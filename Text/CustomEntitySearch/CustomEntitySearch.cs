// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    /// <summary>
    /// Based on sample custom skill provided in Azure Search. Given a user-defined list of entities,
    /// this function will find all occurences of that entity in some input text.
    /// </summary>
    public static class CustomEntitySearch
    {
        // ** Some global variables used for configuration.**
        // ** Change these values prior to deploying your function **
        public static bool ExactMatchesShouldBeCaseSensitive = false;
        public static string EntityDefinitionLocation = "words.csv"; // other option is "words.json"
        public static readonly int MaxRegexEvalTimeInSeconds = 10;


        /// Entity definition is lazy loaded on first function call. This may result in the
        /// first function call taking several seconds (since it needs to parse entity definitions).
        /// subsequent function calls should be significantly faster
        private static WordLinker _userDefinedEntities = null;
        private static Regex _precompiledExactMatchRegex = null;

        /// <summary>
        /// Find instances of custom entities from either words.csv or words.json
        /// in input text
        /// </summary>
        [FunctionName("custom-entity-search")]
        public static async Task<IActionResult> RunCustomEntitySearch(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Custom Entity Search function: C# HTTP trigger function processed a request.");

            if (_userDefinedEntities == null || _precompiledExactMatchRegex == null
                || executionContext.FunctionName == "unitTestFunction") // always reload data for tests
            {
                _userDefinedEntities = WordLinker.WordLink(EntityDefinitionLocation);
                _precompiledExactMatchRegex = new Regex($@"\b({string.Join("|", _userDefinedEntities.Words.Select(w => Regex.Escape(w)))})\b",
                                                                ExactMatchesShouldBeCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase,
                                                                TimeSpan.FromSeconds(MaxRegexEvalTimeInSeconds));
            }

            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{executionContext.FunctionName} - Invalid request record array.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(executionContext.FunctionName, requestRecords,
                 (inRecord, outRecord) =>
                 {
                     if (!inRecord.Data.ContainsKey("text") || inRecord.Data["text"] == null)
                     {
                         outRecord.Errors.Add(new WebApiErrorWarningContract { Message = "Cannot process record without the given key 'text' with a string value" });
                         return outRecord;
                     }

                     string text = inRecord.Data["text"] as string;
                     IList<string> words = _userDefinedEntities.Words;
                     Dictionary<string, string[]> synonyms = _userDefinedEntities.Synonyms;
                     IList<string>  exactMatches = _userDefinedEntities.ExactMatch;
                     int fuzzyEditDistance = Math.Max(0, _userDefinedEntities.FuzzyEditDistance);
                     bool caseSensitive = _userDefinedEntities.CaseSensitive;

                     var entities = new List<Entity>();
                     var entitiesFound = new HashSet<string>();

                     if (!string.IsNullOrWhiteSpace(text))
                     {
                         // exact matches user regex
                         if (fuzzyEditDistance == 0)
                         {
                             MatchCollection entityMatch = _precompiledExactMatchRegex.Matches(text);
                             if (entityMatch.Count > 0)
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

                                     var userWordMatch = _userDefinedEntities.Words.FirstOrDefault(w => w.Equals(match.Groups[1].Value, StringComparison.InvariantCultureIgnoreCase));
                                     entitiesFound.Add(userWordMatch);
                                 }
                             }
                         }
                         // Fuzzy match uses CalculateDamerauLevenshteinDistance
                         else
                         {
                             foreach (string word in words)
                             {
                                 int wordFuzzyEditDistance = (exactMatches != null && exactMatches.Contains(word)) ? 0 : fuzzyEditDistance;
                                 string normalizedWord = (caseSensitive) ? TrimDelineatingCharacters(word) : TrimDelineatingCharacters(word.ToLower(CultureInfo.CurrentCulture));
                                 if (wordFuzzyEditDistance >= normalizedWord.Length)
                                 {
                                     outRecord.Warnings.Add(new WebApiErrorWarningContract
                                     {
                                         Message = @"The provided fuzzy offset of " + wordFuzzyEditDistance + @", is larger than the length of the provided word, """ + word + @"""."
                                     });
                                     wordFuzzyEditDistance = Math.Max(0, normalizedWord.Length - 1);
                                 }

                                 FindMatches(normalizedWord, word, text, entities, entitiesFound, wordFuzzyEditDistance, caseSensitive);

                                 if (synonyms.TryGetValue(word, out string[] wordSynonyms))
                                 {
                                     foreach (string synonym in wordSynonyms)
                                     {
                                         wordFuzzyEditDistance = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : fuzzyEditDistance;
                                         string normalizedSynonymWord = (caseSensitive) ? TrimDelineatingCharacters(synonym) : TrimDelineatingCharacters(synonym.ToLower(CultureInfo.CurrentCulture));
                                         if (wordFuzzyEditDistance >= synonym.Length)
                                         {
                                             outRecord.Warnings.Add(new WebApiErrorWarningContract
                                             {
                                                 Message = @"The provided fuzzy offset of " + wordFuzzyEditDistance + @", is larger than the length of the provided synonym, """ + synonym + @"""."
                                             });
                                             wordFuzzyEditDistance = Math.Max(0, normalizedSynonymWord.Length - 1);
                                         }

                                         FindMatches(normalizedSynonymWord, word, text, entities, entitiesFound, wordFuzzyEditDistance, caseSensitive);
                                     }
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

        public static void FindMatches(
            string wordToFind,
            string unNormalizedWord,
            string text,
            List<Entity> entities,
            HashSet<string> entitiesFound,
            int wordFuzzyEditDistance,
            bool caseSensitive)
        {
            // find all word start and end locations
            List<int> wordStartLocations = new List<int> { 0 };
            List<int> wordEndLocations = new List<int>();
            string normalizedText = (caseSensitive) ? TrimDelineatingCharacters(text) : TrimDelineatingCharacters(text.ToLower(CultureInfo.CurrentCulture));
            for (int currTextIndex = 0; currTextIndex < normalizedText.Length; currTextIndex++)
            {
                if (normalizedText[currTextIndex].IsDelineating())
                {
                    if (currTextIndex + 1 < normalizedText.Length && !normalizedText[currTextIndex + 1].IsDelineating())
                        wordStartLocations.Add(currTextIndex + 1);
                    if (currTextIndex - 1 >= 0 && !normalizedText[currTextIndex - 1].IsDelineating())
                        wordEndLocations.Add(currTextIndex - 1);
                }
            }
            wordEndLocations.Add(normalizedText.Length - 1);

            // find any text substrings that are within fuzzy distance of this word
            // for every potential start position...
            for (int startPointerIndex = 0; startPointerIndex < wordStartLocations.Count; startPointerIndex++)
            {
                double bestMatchDistance = wordFuzzyEditDistance + 1;
                Entity bestMatchSoFar = null;

                // create a word for every subsequent end position
                for (int endPointerIndex = startPointerIndex; endPointerIndex < wordEndLocations.Count; endPointerIndex++)
                {
                    var potentialMatch = normalizedText.Substring(wordStartLocations[startPointerIndex], wordEndLocations[endPointerIndex] - wordStartLocations[startPointerIndex] + 1);

                    if (potentialMatch.Length - wordToFind.Length > wordFuzzyEditDistance)
                    {
                        // we're considering words that are too long and can never be matches
                        // break to next start position
                        break;
                    }

                    // check if this substring matches the input word
                    double editDistance = CalculateDamerauLevenshteinDistance(potentialMatch, wordToFind);

                    if (editDistance > -1
                        && editDistance <= wordFuzzyEditDistance
                        && editDistance < bestMatchDistance)
                    {
                        bestMatchSoFar = new Entity
                        {
                            Category = "customEntity",
                            Value = potentialMatch,
                            Offset = wordStartLocations[startPointerIndex],
                            Confidence = editDistance
                        };
                    }
                }

                // Done with this start index,
                // Store best match if we have one,
                // and then continue to next possible start location
                if (bestMatchSoFar != null)
                {
                    entities.Add(bestMatchSoFar);
                    entitiesFound.Add(unNormalizedWord);
                }
            }
        }

        /// <summary>
        /// Calculate the Demerau Levenshtein Distance between two strings.
        /// If invalid input is provided, this function returns -1
        /// </summary>
        /// <param name="potentialMatch">first string</param>
        /// <param name="entityToFind">second string</param>
        /// <returns>number of character edits needed to tranform the first string into the second</returns>
        private static double CalculateDamerauLevenshteinDistance(string potentialMatch, string entityToFind)
        {
            if (string.IsNullOrEmpty(potentialMatch))
            {
                return entityToFind?.Length ?? -1;
            }

            if (string.IsNullOrEmpty(entityToFind))
            {
                return potentialMatch?.Length ?? -1;
            }

            double[,] dynamicDistanceCalc = new double[potentialMatch.Length + 1, entityToFind.Length + 1];

            for (int currpotentialEntityMatchIndex = 0; currpotentialEntityMatchIndex <= potentialMatch.Length; currpotentialEntityMatchIndex++)
            {
                dynamicDistanceCalc[currpotentialEntityMatchIndex, 0] = currpotentialEntityMatchIndex;
            }
            for (int currWordIndex = 0; currWordIndex <= entityToFind.Length; currWordIndex++)
            {
                dynamicDistanceCalc[0, currWordIndex] = currWordIndex;
            }

            for (int currpotentialEntityMatchIndex = 0; currpotentialEntityMatchIndex < potentialMatch.Length; currpotentialEntityMatchIndex++)
            {
                for (int currWordIndex = 0; currWordIndex < entityToFind.Length; currWordIndex++)
                {
                    double cost = 0;
                    double accentCost = 1;

                    if (potentialMatch[currpotentialEntityMatchIndex].Equals(entityToFind[currWordIndex]))
                    {
                        cost = 0; // the characters match
                    }
                    else if (CharsAreEqualModuloDiacritics(entityToFind[currWordIndex], potentialMatch[currpotentialEntityMatchIndex]))
                    {
                        cost = .5; // the characters only differ by accent characters
                    }
                    else
                    {
                        cost = 1;
                    }

                    if (potentialMatch[currpotentialEntityMatchIndex].IsAccent() ^ entityToFind[currWordIndex].IsAccent())
                    {
                        accentCost = 0.5; // just adding or removing an accent
                    }

                    // Keep the cheapest 
                    dynamicDistanceCalc[currpotentialEntityMatchIndex + 1, currWordIndex + 1] = 
                        Math.Min(
                            Math.Min(
                                dynamicDistanceCalc[currpotentialEntityMatchIndex, currWordIndex + 1] + accentCost, // deletion
                                dynamicDistanceCalc[currpotentialEntityMatchIndex + 1, currWordIndex] + accentCost), // insertion
                                dynamicDistanceCalc[currpotentialEntityMatchIndex, currWordIndex] + cost); // substitution
                   
                    if (currpotentialEntityMatchIndex > 0 && currWordIndex > 0 && potentialMatch[currpotentialEntityMatchIndex].Equals(entityToFind[currWordIndex - 1]) && entityToFind[currWordIndex].Equals(potentialMatch[currpotentialEntityMatchIndex - 1]))
                    {
                        dynamicDistanceCalc[currpotentialEntityMatchIndex + 1, currWordIndex + 1] = 
                            Math.Min(
                                dynamicDistanceCalc[currpotentialEntityMatchIndex + 1, currWordIndex + 1],
                                dynamicDistanceCalc[currpotentialEntityMatchIndex - 1, currWordIndex - 1] + cost); // transposition
                    }
                }
            }

            return dynamicDistanceCalc[potentialMatch.Length, entityToFind.Length];
        }

        /// <summary>
        /// Trims delineating characters from the beginning and end of some text
        /// </summary>
        /// <param name="textToTrim">the text to trim</param>
        /// <returns>a string with the delineating characters trimmed from the beginning and end</returns>
        public static string TrimDelineatingCharacters(string textToTrim)
        {
            int startIndex = 0;
            int endIndex = textToTrim.Length - 1;

            while (startIndex < textToTrim.Length && textToTrim[startIndex].IsDelineating())
                startIndex++;
            while (endIndex >= 0 && textToTrim[endIndex].IsDelineating())
                endIndex--;

            if (startIndex >= endIndex)
            {
                return string.Empty;
            }

            return textToTrim.Substring(startIndex, endIndex - startIndex + 1);
        }

        /// <summary>
        /// Determines if a given character should be considered a "delineating" character
        /// </summary>
        /// <param name="character">the character to evaluate</param>
        /// <returns>true if the character is delineating</returns>
        public static bool IsDelineating(this char character)
        {
            return (Char.IsWhiteSpace(character)
                    || Char.IsSeparator(character)
                    || Char.IsPunctuation(character));
        }

        /// <summary>
        /// Determines if a given character should be considered an accent (diacritic) character
        /// </summary>
        /// <param name="character">the character to evaluate</param>
        /// <returns>true if the character is diacritic</returns>
        public static bool IsAccent(this char character)
        {
            return Char.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark
                   || Char.GetUnicodeCategory(character) == UnicodeCategory.SpacingCombiningMark;
        }

        /// <summary>
        /// Determine if two characters are equal ignoring diacritics
        /// </summary>
        /// <param name="c1">first character</param>
        /// <param name="c2">second character</param>
        /// <returns>true if they're equal ignoring diacritics</returns>
        public static bool CharsAreEqualModuloDiacritics(char c1, char c2)
        {
            return String.Compare(c1.ToString(), c2.ToString(),
                                    CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0;
        }
    }

}