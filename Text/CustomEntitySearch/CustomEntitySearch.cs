// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Text;

// languages used for Azure Search with Text Analytics:
// el, th, he, tr, cs, hu, ar, ja-jp, fi, da, no, ko, pl, ru, sv, ja, it, pt, fr, es, nl, de, en
// greek, thai, hebrew, turkish, czech, hungarian, arabic, japanese, finnish, danish, norwegian, korean, polish, russian, swedish, japanese (again??), 
// italian, portuguese, french, spanish, dutch, german, english
// unicode blocks in order:
// InThai, InHebrew

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    /// <summary>
    /// Based on sample custom skill provided in Azure Search. Provided a user-defined list of entities
    /// this function determines the Entity first occurrence within a given document. This list of entities
    /// must repeatedly be provided by the user for each document.
    /// </summary>
    public static class CustomEntitySearch
    {
        // Use this to load from "csv" or "json" file 
        public static IList<string> preLoadedWords = WordLinker.WordLink(Environment.CurrentDirectory, "csv").Words;

        private static readonly int MaxRegexEvalTime = 1;
        private static bool substringMatch = false;

        /// <summary>
        /// We assert the following assumptions:
        /// 1. All text files contain characters with unicode encoding
        /// 2. Words can contain special characters and numbers
        /// 3. The provided entities are not case sensitive
        /// </summary>

        [FunctionName("custom-search")]
        public static async Task<IActionResult> RunCustomEntitySearch(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext executionContext)
        {
            log.LogInformation("Custom Entity Search function: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }
            
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                 (inRecord, outRecord) => {
                    if (!inRecord.Data.ContainsKey("text") || inRecord.Data["text"] == null)
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract { Message = "Cannot process record without the given key 'text' with a string value" });
                        return outRecord;
                    }
                    if (!inRecord.Data.ContainsKey("words") && 
                    (inRecord.Data.ContainsKey("synonyms") || inRecord.Data.ContainsKey("exactMatches") || inRecord.Data.ContainsKey("fuzzyMatchOffset")))
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract {
                            Message = "Cannot process record without the given key 'words' in the dictionary"});
                        return outRecord;
                    }
                    string text = inRecord.Data["text"] as string;
                     IList<string> words;
                     if (inRecord.Data.ContainsKey("words") == true)
                     {
                         words = inRecord.GetOrCreateList<List<string>>("words");
                     }
                     else
                     {
                         outRecord.Warnings.Add(new WebApiErrorWarningContract
                         {
                             Message = "Used predefined key words from customLookupSkill configuration file " +
                                "since no 'words' parameter was supplied in web request"
                         });
                         words = preLoadedWords;
                     }
                     Dictionary<string, string[]> synonyms = inRecord.GetOrCreateDictionary<Dictionary<string, string[]>>("synonyms");
                     IList<string> exactMatches = inRecord.GetOrCreateList<List<string>>("exactMatches");
                     int offset = (inRecord.Data.ContainsKey("fuzzyMatchOffset")) ? Math.Max(0, Convert.ToInt32(inRecord.Data["fuzzyMatchOffset"])) : 0;
                     bool caseSensitive = (inRecord.Data.ContainsKey("caseSensitive")) ? (bool)inRecord.Data.ContainsKey("caseSensitive") : false;
                     if (words.Count == 0 || (words.Count(word => !String.IsNullOrEmpty(word)) == 0))
                     {
                        try
                        {
                            WordLinker userInput = WordLinker.WordLink(executionContext.FunctionDirectory, "json");
                            words = userInput.Words;
                            synonyms = userInput.Synonyms;
                            exactMatches = userInput.ExactMatch;
                            offset = (userInput.FuzzyMatchOffset >= 0) ? userInput.FuzzyMatchOffset : 0;
                            caseSensitive = userInput.CaseSensitive;
                            outRecord.Warnings.Add(new WebApiErrorWarningContract
                            {
                                Message = "Used predefined key words from customLookupSkill configuration file " +
                            "since no 'words' parameter was supplied in web request"
                            });
                         }
                        catch (Exception)
                        {
                            outRecord.Errors.Add(new WebApiErrorWarningContract
                            {
                                Message = "Could not parse predefined words.json"
                            });
                            return outRecord;
                        }
                    }

                    var entities = new List<Entity>();
                    var entitiesFound = new HashSet<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        foreach (string word in words)
                        {
                            if (string.IsNullOrEmpty(word)) continue;
                            int leniency = (exactMatches != null && exactMatches.Contains(word)) ? 0 : offset;
                            string wordCharArray = (caseSensitive) ? CreateWordArray(word) : CreateWordArray(word.ToLower(CultureInfo.CurrentCulture));
                             if (leniency >= wordCharArray.Length)
                             {
                                 outRecord.Warnings.Add(new WebApiErrorWarningContract
                                 {
                                     Message = @"The provided fuzzy offset of " + leniency + @", is larger than the length of the provided word, """ + word + @"""."
                                 });
                                 leniency = Math.Max(0, wordCharArray.Length - 1);
                             }
                             if (AddValues(word, text, wordCharArray, entities, leniency, caseSensitive))
                                 entitiesFound.Add(word);
                            if (synonyms.TryGetValue(word, out string[] wordSynonyms))
                            {
                                foreach (string synonym in wordSynonyms)
                                {
                                    leniency = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : offset;
                                    string synonymCharArray = (caseSensitive) ? CreateWordArray(synonym) : CreateWordArray(synonym.ToLower(CultureInfo.CurrentCulture));
                                     if (leniency >= synonym.Length)
                                     {
                                        outRecord.Warnings.Add(new WebApiErrorWarningContract
                                        {
                                            Message = @"The provided fuzzy offset of " + leniency + @", is larger than the length of the provided synonym, """ + synonym + @"""."
                                        });
                                        leniency = Math.Max(0, synonymCharArray.Length - 1);
                                     }
                                     if (AddValues(synonym, text, synonymCharArray, entities, leniency, caseSensitive))
                                         entitiesFound.Add(synonym);
                                }
                            }
                        }
                    }

                    outRecord.Data["Entities"] = entities;
                    outRecord.Data["EntitiesFound"] = entitiesFound;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        public static bool AddValues(
            string checkMatch, 
            string text, 
            string word, 
            List<Entity> entities, 
            int leniency, 
            bool caseSensitive)
        {
            bool addToEntitiesFound = false;
            if (leniency == 0)
            {
                // Overlap checker now also included in Regex expression using delineating characters as overlap lookahead
                StringBuilder escapedWord = new StringBuilder(@"(?=(");
                if (!word.First().IsDelineating() && !substringMatch)
                    escapedWord.Append(@"\b");
                for (int currWordCharIndex = 0; currWordCharIndex < word.Length; currWordCharIndex++)
                {
                    if (word[currWordCharIndex].IsDelineating())
                    {
                        escapedWord.Append(".");
                    }
                    else
                    {
                        escapedWord.Append(word[currWordCharIndex]);
                    }
                }
                if (!word.Last().IsDelineating() && !substringMatch)
                    escapedWord.Append(@"\b");
                escapedWord.Append("))");
                string pattern = (caseSensitive) ? @"(?x)" + escapedWord : @"(?ix)" + escapedWord;
                
                MatchCollection entityMatch = Regex.Matches(text, pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(MaxRegexEvalTime));
                if (entityMatch.Count != 0)
                {
                    foreach (Match match in entityMatch)
                    {
                        entities.Add(
                            new Entity
                            {
                                Category = "customEntity",
                                Value = match.Groups[1].Value,
                                Offset = match.Index,
                                Confidence = 0
                            });
                    }
                    addToEntitiesFound = true;
                }
            }
            else
            {
                List<int> startPointersInText = new List<int> { 0 };
                List<int> endPointersInText = new List<int>();
                string textCharArray = (caseSensitive) ? CreateWordArray(text) : CreateWordArray(text.ToLower(CultureInfo.CurrentCulture));
                for (int currTextCharIndex = 0; currTextCharIndex < textCharArray.Length; currTextCharIndex++)
                {
                    if (textCharArray[currTextCharIndex].IsDelineating())
                    {
                        if (currTextCharIndex + 1 < textCharArray.Length && !textCharArray[currTextCharIndex + 1].IsDelineating())
                            startPointersInText.Add(currTextCharIndex + 1);
                        if (currTextCharIndex - 1 >= 0 && !textCharArray[currTextCharIndex - 1].IsDelineating())
                            endPointersInText.Add(currTextCharIndex - 1);
                    }
                }
                endPointersInText.Add(textCharArray.Length - 1);

                double[] minLevenshteinDistance = new double[startPointersInText.Count];
                int[] endofMatchInTextPointer = new int[startPointersInText.Count];
                for (int startPointerIndex = 0; startPointerIndex < startPointersInText.Count; startPointerIndex++)
                {
                    minLevenshteinDistance[startPointerIndex] = leniency + 1;
                    for (int endPointerIndex = startPointerIndex; endPointerIndex < endPointersInText.Count; endPointerIndex++)
                    {
                        if (endPointersInText[endPointerIndex] - startPointersInText[startPointerIndex] + 1 > checkMatch.Length + leniency) break;
                        double distance = DamerauLevenshteinCalculation(textCharArray.Substring(startPointersInText[startPointerIndex],
                            endPointersInText[endPointerIndex] - startPointersInText[startPointerIndex] + 1), word);
                        if (distance > -1 && minLevenshteinDistance[startPointerIndex] > distance)
                        {
                            minLevenshteinDistance[startPointerIndex] = distance;
                            endofMatchInTextPointer[startPointerIndex] = endPointerIndex;
                        }
                    }
                }

                for (int i = 0; i < minLevenshteinDistance.Length; i++)
                {
                    if (minLevenshteinDistance[i] <= leniency)
                    {
                        entities.Add(
                                new Entity
                                {
                                    Category = "customEntity",
                                    Value = text.Substring(startPointersInText[i], endPointersInText[endofMatchInTextPointer[i]] - startPointersInText[i] + 1),
                                    Offset = startPointersInText[i],
                                    Confidence = minLevenshteinDistance[i]
                                });
                        addToEntitiesFound = true;
                    }
                }
            }
            return addToEntitiesFound;
        }

        private static double DamerauLevenshteinCalculation(string text, string checkMatch)
        {
            double[,] dynamicDistanceCalc = new double[text.Length + 1, checkMatch.Length + 1];
            double substitutionCost = -1;
            double accentAddition = 1;
            for (int currTextIndex = 0; currTextIndex <= text.Length; currTextIndex++)
                dynamicDistanceCalc[currTextIndex, 0] = currTextIndex;
            for (int currWordIndex = 0; currWordIndex <= checkMatch.Length; currWordIndex++)
                dynamicDistanceCalc[0, currWordIndex] = currWordIndex;

            for (int currTextIndex = 0; currTextIndex < text.Length; currTextIndex++)
            {
                for (int currWordIndex = 0; currWordIndex < checkMatch.Length; currWordIndex++)
                {
                    if (text[currTextIndex].Equals(checkMatch[currWordIndex]))
                        substitutionCost = 0;
                    else if (String.Compare(checkMatch[currWordIndex].ToString(), text[currTextIndex].ToString(),
                                    CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)
                        substitutionCost = .5;
                    else
                        substitutionCost = 1;
                    if (checkMatch[currWordIndex].IsAccent() ^ text[currTextIndex].IsAccent())
                        accentAddition = 0.5;
                    dynamicDistanceCalc[currTextIndex + 1, currWordIndex + 1] = Math.Min(
                        Math.Min(dynamicDistanceCalc[currTextIndex, currWordIndex + 1] + accentAddition, // deletion
                        dynamicDistanceCalc[currTextIndex + 1, currWordIndex] + accentAddition), // insertion
                        dynamicDistanceCalc[currTextIndex, currWordIndex] + substitutionCost); // substitution
                    if (currTextIndex > 0 && currWordIndex > 0 && text[currTextIndex].Equals(checkMatch[currWordIndex - 1]) && checkMatch[currWordIndex].Equals(text[currTextIndex - 1]))
                        dynamicDistanceCalc[currTextIndex + 1, currWordIndex + 1] = Math.Min(dynamicDistanceCalc[currTextIndex + 1, currWordIndex + 1], 
                            dynamicDistanceCalc[currTextIndex - 1, currWordIndex - 1] + substitutionCost); // transposition
                }
            }

            return dynamicDistanceCalc[text.Length, checkMatch.Length];
        }

        /*
        * Given an entity the user wants to find, this method removes delineating characters if they are found in the
        * beginning or end of the entity definition. The method then returns the exact word that will be used for fuzzy matching
        */
        public static string CreateWordArray(string checkMatch)
        {
            int initCheckIndex = 0;
            int endCheckIndex = checkMatch.Length - 1;

            while (initCheckIndex < checkMatch.Length && checkMatch[initCheckIndex].IsDelineating())
                initCheckIndex++;
            while (endCheckIndex >= 0 && checkMatch[endCheckIndex].IsDelineating())
                endCheckIndex--;
            if (initCheckIndex <= endCheckIndex && (initCheckIndex != 0 || endCheckIndex != checkMatch.Length - 1))
            {
                return checkMatch.Substring(initCheckIndex, endCheckIndex - initCheckIndex + 1);
            }

            return checkMatch;
        }

        public static bool IsDelineating(this char checkSymbol)
        {
            return (Char.IsWhiteSpace(checkSymbol) || Char.IsSeparator(checkSymbol) || Char.IsPunctuation(checkSymbol));
        }
        public static bool IsAccent(this char checkSymbol)
        {
            return Char.GetUnicodeCategory(checkSymbol) == UnicodeCategory.NonSpacingMark || Char.GetUnicodeCategory(checkSymbol) == UnicodeCategory.SpacingCombiningMark;
        }
        public static T GetOrCreateList<T>(this WebApiRequestRecord record, string propertyName) 
            where T : class, IEnumerable, new() => (record.Data.TryGetValue(propertyName, out object objectValue) ? 
            ((JArray)objectValue).ToObject<T>() : new T()) ?? new T();
        public static T GetOrCreateDictionary<T>(this WebApiRequestRecord record, string propertyName)
            where T : class, IEnumerable, new() => (record.Data.TryGetValue(propertyName, out object objectValue) ?
            ((JContainer)objectValue).ToObject<T>() : new T()) ?? new T();
    }

}