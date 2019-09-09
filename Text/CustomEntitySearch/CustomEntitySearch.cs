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
        private static int MaxRegexEvalTime = 1;
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
                    IList<string> words = inRecord.GetOrCreateList<List<string>>("words");
                    Dictionary<string, string[]> synonyms = inRecord.GetOrCreateDictionary<Dictionary<string, string[]>>("synonyms");
                    IList<string> exactMatches = inRecord.GetOrCreateList<List<string>>("exactMatches");
                    int offset = (inRecord.Data.ContainsKey("fuzzyMatchOffset")) ? Math.Max(0, Convert.ToInt32(inRecord.Data["fuzzyMatchOffset"])) : 0;
                    bool caseSensitive = (inRecord.Data.ContainsKey("caseSensitive")) ? (bool)inRecord.Data.ContainsKey("caseSensitive") : false;
                    if (words.Count == 0 || (words.Count(word => !String.IsNullOrEmpty(word)) == 0))
                    {
                        outRecord.Warnings.Add(new WebApiErrorWarningContract {
                            Message = "Used predefined key words from customLookupSkill configuration file " +
                            "since no 'words' parameter was supplied in web request" });
                        WordLinker userInput = WordLinker.WordLink(executionContext.FunctionAppDirectory);
                        words = userInput.Words;
                        synonyms = userInput.Synonyms;
                        exactMatches = userInput.ExactMatch;
                        offset = (userInput.FuzzyMatchOffset >= 0) ? userInput.FuzzyMatchOffset : 0;
                        caseSensitive = userInput.CaseSensitive;
                    }

                    var entities = new List<Entity>();
                    var entitiesFound = new HashSet<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        foreach (string word in words)
                        {
                            if (string.IsNullOrEmpty(word)) continue;
                            int leniency = (exactMatches != null && exactMatches.Contains(word)) ? 0 : offset;
                            if (leniency >= word.Length)
                             {
                                 outRecord.Warnings.Add(new WebApiErrorWarningContract
                                 {
                                     Message = @"The provided fuzzy offset of " + leniency + @", is larger than the length of the provided word, """ + word + @"""."
                                 });
                             }
                            AddValues(word, text, entities, entitiesFound, leniency, caseSensitive);
                            if (synonyms.TryGetValue(word, out string[] wordSynonyms))
                            {
                                foreach (string synonym in wordSynonyms)
                                {
                                    leniency = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : offset;
                                    if (leniency >= synonym.Length)
                                    {
                                        outRecord.Warnings.Add(new WebApiErrorWarningContract
                                        {
                                            Message = @"The provided fuzzy offset of " + leniency + @", is larger than the length of the provided synonym, """ + synonym + @"""."
                                        });
                                    }
                                     AddValues(synonym, text, entities, entitiesFound, leniency, caseSensitive);
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

        public static void AddValues(string checkMatch, string text, List<Entity> entities, 
            HashSet<string> entitiesFound, int leniency, bool caseSensitive)
        {
            if (leniency == 0)
            {
                string escapedWord = Regex.Escape(checkMatch);
                string pattern = (caseSensitive) ? @"(?x:" + escapedWord + @")" : @"(?ix:" + escapedWord + @")";
                if (!escapedWord.First().IsDelineating() && !substringMatch)
                    pattern = @"\b" + pattern;
                if (!escapedWord.Last().IsDelineating() && !substringMatch)
                    pattern += @"\b";
                MatchCollection entityMatch = Regex.Matches(text, pattern, RegexOptions.Compiled, TimeSpan.FromSeconds(MaxRegexEvalTime));
                if (entityMatch.Count != 0)
                {
                    foreach (Match match in entityMatch)
                    {
                        entities.Add(
                            new Entity
                            {
                                Category = "customEntity",
                                Value = match.Value,
                                Offset = match.Index,
                                Confidence = 0
                            });
                    }
                    entitiesFound.Add(checkMatch);
                }
            }
            else
            {
                // Begin searching!
                int bestMismatchPreWhitespace = -1;
                int currWordCharIndex = 0;
                string wordCharArray = (caseSensitive) ? CreateWordArray(checkMatch) : CreateWordArray(checkMatch.ToLower(CultureInfo.CurrentCulture));
                string textCharArray = (caseSensitive) ? text : text.ToLower(CultureInfo.CurrentCulture);
                if (leniency >= wordCharArray.Length)
                    leniency = wordCharArray.Length - 2;

                if (leniency >= 0)
                {
                    PotentialEntity.Clear();
                    while (PotentialEntity.GetEndIndex() < textCharArray.Length)
                    {
                        // Skip past extra delineating characters in the front of the word in the text
                        if (currWordCharIndex == 0 && PotentialEntity.mismatchScore >= 0 && textCharArray[PotentialEntity.GetEndIndex()].IsDelineating())
                        {
                            PotentialEntity.ResetPotentialEntity();
                            continue;
                        }

                        if (PotentialEntity.mismatchScore - PotentialEntity.CheckDiff() <= leniency)
                        {
                            if (wordCharArray[currWordCharIndex] == textCharArray[PotentialEntity.GetEndIndex()])
                            {
                                if (textCharArray[PotentialEntity.GetEndIndex()].IsDelineating() &&
                                    (bestMismatchPreWhitespace == -1 || 
                                    bestMismatchPreWhitespace > PotentialEntity.mismatchScore + (wordCharArray.Length - currWordCharIndex)))
                                {
                                    bestMismatchPreWhitespace = (int)PotentialEntity.mismatchScore + (wordCharArray.Length - currWordCharIndex);
                                }
                                PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()]);
                                currWordCharIndex++;
                            }
                            else
                            {
                                double potTextMismatch = (textCharArray[PotentialEntity.GetEndIndex()].IsAccent()) ? 0.5 : 0;
                                double potWordMismatch = (wordCharArray[currWordCharIndex].IsAccent()) ? 0.5 : 0;
                                // fuzzy situation?
                                // accent case adds 0.5
                                if (String.Compare(wordCharArray[currWordCharIndex].ToString(), textCharArray[PotentialEntity.GetEndIndex()].ToString(),
                                    CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0)
                                {
                                    PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 0.5);
                                    currWordCharIndex++;
                                }
                                else if (potWordMismatch > 0 && potTextMismatch == 0)
                                {
                                    PotentialEntity.mismatchScore += 0.5;
                                    currWordCharIndex++;
                                }
                                else if (potWordMismatch == 0 && potTextMismatch > 0)
                                {
                                    PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 0.5);
                                }
                                else
                                {
                                    int offsetWord = 0;
                                    bool trueWordComp = false;
                                    bool trueTextComp = false;
                                    for (int i = currWordCharIndex + 1; i < wordCharArray.Length; i++)
                                    {
                                        if (potWordMismatch >= leniency - PotentialEntity.mismatchScore)
                                            break;
                                        potWordMismatch++;
                                        offsetWord++;
                                        if (String.Compare(wordCharArray[i].ToString(), 
                                            textCharArray[PotentialEntity.GetEndIndex()].ToString(), CultureInfo.CurrentCulture,
                                                CompareOptions.IgnoreNonSpace) == 0)
                                        {
                                            trueWordComp = true;
                                            break;
                                        }
                                    }
                                    for (int i = PotentialEntity.GetEndIndex() + 1; i < textCharArray.Length; i++)
                                    {
                                        if (potTextMismatch >= leniency - PotentialEntity.mismatchScore)
                                            break;
                                        if (currWordCharIndex == wordCharArray.Length - 1 &&
                                            Char.GetUnicodeCategory(wordCharArray[currWordCharIndex]) != Char.GetUnicodeCategory(textCharArray[i]))
                                            break;
                                        potTextMismatch++;
                                        if (String.Compare(wordCharArray[currWordCharIndex].ToString(), textCharArray[i].ToString(), CultureInfo.CurrentCulture,
                                                CompareOptions.IgnoreNonSpace) == 0)
                                        {
                                            trueTextComp = true;
                                            if (textCharArray[i - 1].IsDelineating() &&
                                                (bestMismatchPreWhitespace == -1 || 
                                                bestMismatchPreWhitespace > (int)PotentialEntity.mismatchScore + (wordCharArray.Length - currWordCharIndex)))
                                            {
                                                bestMismatchPreWhitespace = (int)PotentialEntity.mismatchScore + (wordCharArray.Length - currWordCharIndex);
                                            }
                                            break;
                                        }
                                    }

                                    if (trueTextComp && trueWordComp)
                                    {
                                        if (potWordMismatch == potTextMismatch)
                                        {
                                            PotentialEntity.AddToDiff(wordCharArray[currWordCharIndex], textCharArray[PotentialEntity.GetEndIndex()]);
                                            PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 1);
                                            currWordCharIndex += 1;
                                        }
                                        else if ((potWordMismatch > potTextMismatch && potTextMismatch != 0) || potWordMismatch == 0)
                                        {
                                            PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 1);
                                        }
                                        else
                                        {
                                            PotentialEntity.mismatchScore += potWordMismatch;
                                            currWordCharIndex += offsetWord;
                                        }
                                    }
                                    else if (trueTextComp)
                                    {
                                        PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 1);
                                    }
                                    else if (trueWordComp)
                                    {
                                        PotentialEntity.mismatchScore += potWordMismatch;
                                        currWordCharIndex += offsetWord;
                                    }
                                    else
                                    {
                                        PotentialEntity.AddToDiff(wordCharArray[currWordCharIndex], textCharArray[PotentialEntity.GetEndIndex()]);
                                        PotentialEntity.mismatchScore += 1;
                                        currWordCharIndex++;
                                        if (!(currWordCharIndex >= wordCharArray.Length && 
                                            textCharArray[PotentialEntity.GetEndIndex()].IsDelineating()) && 
                                            PotentialEntity.mismatchScore - PotentialEntity.CheckDiff() <= leniency)
                                        {
                                            PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()]);
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            currWordCharIndex = 0;
                            if (bestMismatchPreWhitespace > -1 && PotentialEntity.GetEndIndex() - bestMismatchPreWhitespace < PotentialEntity.GetStartIndex() - leniency)
                                PotentialEntity.ResetPotentialEntity(-1 * (bestMismatchPreWhitespace - 1));
                            else
                                PotentialEntity.ResetPotentialEntity(0);
                            bestMismatchPreWhitespace = -1;
                        }
                        if (currWordCharIndex >= wordCharArray.Length && !textCharArray[PotentialEntity.GetEndIndex() - 1].IsDelineating())
                        {
                            while (PotentialEntity.GetEndIndex() < textCharArray.Length)
                            {
                                if (!textCharArray[PotentialEntity.GetEndIndex()].IsDelineating())
                                {
                                    PotentialEntity.MatchInText(text[PotentialEntity.GetEndIndex()], 1);
                                }
                                else
                                    break;
                            }
                        }
                        if (PotentialEntity.GetEndIndex() >= textCharArray.Length || 
                            (currWordCharIndex >= wordCharArray.Length && textCharArray[PotentialEntity.GetEndIndex()].IsDelineating()))
                        {
                            PotentialEntity.mismatchScore += Math.Max(0, wordCharArray.Length - currWordCharIndex);
                            PotentialEntity.mismatchScore -= PotentialEntity.CheckDiff();
                            if (PotentialEntity.mismatchScore <= leniency)
                            {
                                if (bestMismatchPreWhitespace > -1 && bestMismatchPreWhitespace < PotentialEntity.mismatchScore)
                                {
                                    entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = PotentialEntity.GetMatch((int) bestMismatchPreWhitespace),
                                        Offset = PotentialEntity.GetStartIndex(),
                                        Confidence = bestMismatchPreWhitespace + (wordCharArray.Length - currWordCharIndex)
                                    });
                                }
                                else
                                {
                                    entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = PotentialEntity.GetMatch(),
                                        Offset = PotentialEntity.GetStartIndex(),
                                        Confidence = PotentialEntity.mismatchScore + (wordCharArray.Length - currWordCharIndex)
                                    });
                                }

                                entitiesFound.Add(checkMatch);
                                currWordCharIndex = 0;
                                if (bestMismatchPreWhitespace > -1 && bestMismatchPreWhitespace < PotentialEntity.mismatchScore
                                    && PotentialEntity.GetEndIndex() - bestMismatchPreWhitespace != PotentialEntity.GetStartIndex() - 1)
                                    PotentialEntity.ResetPotentialEntity(-1 * (bestMismatchPreWhitespace - 1));
                                else
                                    PotentialEntity.ResetPotentialEntity(0);
                                bestMismatchPreWhitespace = -1;
                            }
                        }
                    }
                }
            }
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
            if (initCheckIndex != 0 || endCheckIndex != checkMatch.Length - 1)
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