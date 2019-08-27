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
                    if (!inRecord.Data.ContainsKey("text"))
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract { Message = "The given key 'text' was not present in the dictionary." });
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
                    IList<string> words = (inRecord.Data.ContainsKey("words")) ? 
                    ((JArray)inRecord.Data["words"]).ToObject<List<string>>() : new List<string>();
                    Dictionary<string, string[]> synonyms = (inRecord.Data.ContainsKey("synonyms")) ? 
                    ((JContainer)inRecord.Data["synonyms"]).ToObject<Dictionary<string, string[]>>() : new Dictionary<string, string[]>();
                    IList<string> exactMatches = (inRecord.Data.ContainsKey("exactMatches")) ? 
                    ((JArray)inRecord.Data["exactMatches"]).ToObject<List<string>>() : new List<string>();
                    int offset = (inRecord.Data.ContainsKey("fuzzyMatchOffset") && (int)(long)inRecord.Data["fuzzyMatchOffset"] >= 0) ? 
                    (int)(long)inRecord.Data["fuzzyMatchOffset"] : 0;
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
                            AddValues(word, text, entities, entitiesFound, leniency, caseSensitive);
                            if (synonyms.TryGetValue(word, out string[] wordSynonyms))
                            {
                                foreach (string synonym in wordSynonyms)
                                {
                                    leniency = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : offset;
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
            bool addWord = false;

            if (leniency == 0 || leniency >= checkMatch.Length)
            {
                string escapedWord = Regex.Escape(checkMatch);
                string pattern = (caseSensitive) ? @"(?x:" + escapedWord + @")" : @"(?ix:" + escapedWord + @")";
                if (!(Char.IsPunctuation(escapedWord.First()) || Char.IsWhiteSpace(escapedWord.First())) && leniency < checkMatch.Length)
                    pattern = @"\b" + pattern;
                if (!(Char.IsPunctuation(escapedWord.Last()) || Char.IsWhiteSpace(escapedWord.Last())) && leniency < checkMatch.Length)
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
                                Confidence = 1
                            });
                    }
                    addWord = true;
                }
            }

            if (leniency > 0 && leniency < checkMatch.Length)
            {
                // Begin searching!
                int whitespaceOffset = 0;
                double bestMismatchPreWhitespace = -1;
                int prevWhiteSpaceIndex = 0;
                int currTextCharIndex = 0;
                int currWordCharIndex = 0;
                StringWriter wordFound = new StringWriter();
                double currMismatch = 0;
                IList<char> wordCharArray = (caseSensitive) ? CreateWordArray(checkMatch) : CreateWordArray(checkMatch.ToLower());
                IList<char> textCharArray = (caseSensitive) ? text.ToCharArray() : text.ToLower().ToCharArray();

                while (currTextCharIndex < textCharArray.Count)
                {
                    // First 
                    if ((currWordCharIndex == 0 || currMismatch > 0) && 
                        (Char.IsWhiteSpace(textCharArray[currTextCharIndex]) || Char.IsSeparator(textCharArray[currTextCharIndex]) 
                        || Char.IsPunctuation(textCharArray[currTextCharIndex])))
                        prevWhiteSpaceIndex = currTextCharIndex;
                    if (currWordCharIndex == 0 && currMismatch > 0 
                        && Char.GetUnicodeCategory(textCharArray[currTextCharIndex - 1]) != Char.GetUnicodeCategory(wordCharArray[currWordCharIndex]))
                    {
                        wordFound.GetStringBuilder().Clear();
                        currMismatch = 0;
                    }
                    if (currMismatch <= leniency)
                    {
                        if (wordCharArray[currWordCharIndex] == textCharArray[currTextCharIndex])
                        {
                            if (Char.IsWhiteSpace(wordCharArray[currWordCharIndex]))
                            {
                                whitespaceOffset = currTextCharIndex;
                                bestMismatchPreWhitespace = currMismatch + (wordCharArray.Count - currWordCharIndex);
                            }
                            wordFound.Write(text.ElementAt(currTextCharIndex));
                            currTextCharIndex++;
                            currWordCharIndex++;
                        }
                        else
                        {
                            double potTextMismatch = (Char.GetUnicodeCategory(textCharArray[currTextCharIndex]) == UnicodeCategory.NonSpacingMark ||
                                        Char.GetUnicodeCategory(textCharArray[currTextCharIndex]) == UnicodeCategory.SpacingCombiningMark) ? 0.5 : 0;
                            double potWordMismatch = (Char.GetUnicodeCategory(wordCharArray[currWordCharIndex]) == UnicodeCategory.NonSpacingMark ||
                                        Char.GetUnicodeCategory(wordCharArray[currWordCharIndex]) == UnicodeCategory.SpacingCombiningMark) ? 0.5 : 0;
                            // fuzzy situation?
                            // accent case adds 0.5
                            if (String.Compare(wordCharArray[currWordCharIndex].ToString(), textCharArray[currTextCharIndex].ToString(),
                                CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                            {
                                currMismatch += 0.5;
                                wordFound.Write(text.ElementAt(currTextCharIndex));
                                currTextCharIndex++;
                                currWordCharIndex++;
                            }
                            else if (potWordMismatch > 0 && potTextMismatch == 0)
                            {
                                currMismatch += 0.5;
                                currWordCharIndex++;
                            }
                            else if (potWordMismatch == 0 && potTextMismatch > 0)
                            {
                                currMismatch += 0.5;
                                wordFound.Write(text.ElementAt(currTextCharIndex));
                                currTextCharIndex++;
                            }
                            else
                            {
                                int offsetText = 0;
                                int offsetWord = 0;
                                bool trueWordComp = false;
                                bool trueTextComp = false;
                                if (currWordCharIndex < wordCharArray.Count - 1)
                                {
                                    for (int i = currWordCharIndex + 1; i < wordCharArray.Count; i++)
                                    {
                                        if (potWordMismatch >= leniency - currMismatch)
                                            break;
                                        potWordMismatch++;
                                        offsetWord++;
                                        if (String.Compare(wordCharArray[i].ToString(), textCharArray[currTextCharIndex].ToString(), CultureInfo.CurrentCulture,
                                             CompareOptions.IgnoreNonSpace) == 0)
                                        {
                                            trueWordComp = true;
                                            break;
                                        }
                                    }
                                }
                                if (currTextCharIndex < textCharArray.Count - 1)
                                {
                                    for (int i = currTextCharIndex + 1; i < textCharArray.Count; i++)
                                    {
                                        if (potTextMismatch >= leniency - currMismatch)
                                            break;
                                        if (currWordCharIndex == wordCharArray.Count - 1 &&
                                            Char.GetUnicodeCategory(wordCharArray[currWordCharIndex]) != Char.GetUnicodeCategory(textCharArray[i]))
                                            break;
                                        potTextMismatch++;
                                        offsetText++;
                                        if (String.Compare(wordCharArray[currWordCharIndex].ToString(), textCharArray[i].ToString(), CultureInfo.CurrentCulture,
                                             CompareOptions.IgnoreNonSpace) == 0)
                                        {
                                            trueTextComp = true;
                                            if (Char.IsWhiteSpace(textCharArray[i-1]) || Char.IsSeparator(textCharArray[i-1]))
                                            {
                                                if (bestMismatchPreWhitespace == -1 || bestMismatchPreWhitespace > currMismatch + (wordCharArray.Count - currWordCharIndex))
                                                {
                                                    whitespaceOffset = i;
                                                    bestMismatchPreWhitespace = currMismatch + (wordCharArray.Count - currWordCharIndex);
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                                else
                                    potWordMismatch = potTextMismatch;

                                if (trueTextComp && trueWordComp)
                                {
                                    if (potWordMismatch == potTextMismatch)
                                    {
                                        currMismatch += 1;
                                        wordFound.Write(text.ElementAt(currTextCharIndex));
                                        currTextCharIndex++;
                                        currWordCharIndex++;
                                    }
                                    else if ((potWordMismatch > potTextMismatch && potTextMismatch != 0) || potWordMismatch == 0)
                                    {
                                        currMismatch += 1;
                                        wordFound.Write(text.ElementAt(currTextCharIndex));
                                        currTextCharIndex++;
                                    }
                                    else
                                    {
                                        currMismatch += potWordMismatch;
                                        currWordCharIndex += offsetWord;
                                    }
                                }
                                else if (trueTextComp)
                                {
                                    currMismatch += 1;
                                    wordFound.Write(text.ElementAt(currTextCharIndex));
                                    currTextCharIndex++;
                                }
                                else if (trueWordComp)
                                {
                                    currMismatch += potWordMismatch;
                                    currWordCharIndex += offsetWord;
                                }
                                else
                                {
                                    currMismatch += 1;
                                    wordFound.Write(text.ElementAt(currTextCharIndex));
                                    if (currMismatch <= leniency)
                                        currTextCharIndex++;
                                    currWordCharIndex++;
                                }
                                
                            }
                        }
                        int initialOffsetIndex = currTextCharIndex - wordFound.ToString().Length;
                        int finalOffsetIndex = (initialOffsetIndex - prevWhiteSpaceIndex - 1 <= 0) ? initialOffsetIndex : prevWhiteSpaceIndex + 1;
                        double secondCheck = (initialOffsetIndex - prevWhiteSpaceIndex - 1 <= 0) ? currMismatch : initialOffsetIndex - prevWhiteSpaceIndex - 1 + currMismatch;
                        if (((currTextCharIndex >= textCharArray.Count && currMismatch + (wordCharArray.Count - currWordCharIndex) <= leniency)
                            || currWordCharIndex >= wordCharArray.Count) && (secondCheck <= leniency))
                        {
                            if (currWordCharIndex >= wordCharArray.Count && currTextCharIndex < textCharArray.Count
                                && Char.IsLetterOrDigit(wordCharArray.Last<char>()))
                            {
                                if (!Char.IsWhiteSpace(textCharArray[currTextCharIndex - 1]))
                                {
                                    while (currTextCharIndex < textCharArray.Count)
                                    {
                                        if (Char.IsLetterOrDigit(textCharArray[currTextCharIndex]))
                                        {
                                            wordFound.Write(textCharArray[currTextCharIndex]);
                                            currTextCharIndex++;
                                            secondCheck++;
                                        }
                                        else
                                            break;
                                    }
                                }
                            }
                            int addToWord = initialOffsetIndex - finalOffsetIndex;
                            if (addToWord > 0)
                            {
                                // write prefix before hand to reduce number of insertions into StringWriter
                                List<char> prefix = new List<char>();
                                while (addToWord > 0)
                                {
                                    prefix.Add(textCharArray[initialOffsetIndex - addToWord]);
                                    addToWord--;
                                }
                                wordFound.GetStringBuilder().Insert(0, prefix.ToArray());
                            }
                            // Code Cleanup?
                            if (secondCheck <= leniency)
                            {
                                if (bestMismatchPreWhitespace > -1 && bestMismatchPreWhitespace < secondCheck)
                                {
                                    entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = wordFound.GetStringBuilder().Remove(whitespaceOffset - finalOffsetIndex - 1, (int)bestMismatchPreWhitespace + 1).ToString(),
                                        Offset = finalOffsetIndex,
                                        Confidence = leniency - bestMismatchPreWhitespace - (wordCharArray.Count - currWordCharIndex)
                                    });
                                    currTextCharIndex = whitespaceOffset;
                                }
                                else
                                {
                                    entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = wordFound.ToString(),
                                        Offset = finalOffsetIndex,
                                        Confidence = leniency - secondCheck - (wordCharArray.Count - currWordCharIndex)
                                    });

                                    if (bestMismatchPreWhitespace > -1 && whitespaceOffset != initialOffsetIndex)
                                    {
                                        currTextCharIndex = whitespaceOffset;
                                    }
                                }
                                
                                addWord = true;
                                currWordCharIndex = 0;
                                wordFound.GetStringBuilder().Clear();
                                currMismatch = 0;
                                bestMismatchPreWhitespace = -1;
                            }
                            else
                                currMismatch = leniency + 1;
                        }
                        else if (currWordCharIndex >= wordCharArray.Count && secondCheck > leniency)
                        {
                            currMismatch = leniency + 1;
                        }
                    }
                    else
                    {
                        int initialOffsetIndex = currTextCharIndex - wordFound.ToString().Length;
                        currWordCharIndex = 0;
                        currTextCharIndex++;
                        if (bestMismatchPreWhitespace > -1 && whitespaceOffset != initialOffsetIndex)
                        {
                            currTextCharIndex = whitespaceOffset;
                        }
                        wordFound.GetStringBuilder().Clear();
                        currMismatch = 0;
                        bestMismatchPreWhitespace = -1;
                    }
                }
            }
            if (addWord)
                entitiesFound.Add(checkMatch);
        }

        public static IList<char> CreateWordArray(string checkMatch)
        {
            int initCheckIndex = 0;
            int endCheckIndex = checkMatch.Length - 1;

            IList<char> wordCharArray = checkMatch.ToCharArray();

            while (initCheckIndex < checkMatch.Length && Char.IsWhiteSpace(wordCharArray[initCheckIndex]))
                initCheckIndex++;
            while (endCheckIndex >= 0 && Char.IsWhiteSpace(wordCharArray[endCheckIndex]))
                endCheckIndex--;
            if (initCheckIndex != 0 || endCheckIndex != checkMatch.Length - 1)
            {
                wordCharArray = wordCharArray.ToList<char>().GetRange(initCheckIndex, endCheckIndex + 1);
            }

            return wordCharArray;
        }
    }
}