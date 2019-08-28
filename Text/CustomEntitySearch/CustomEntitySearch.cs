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
                    IList<string> words = (inRecord.Data.ContainsKey("words")) ? 
                    ((JArray)inRecord.Data["words"]).ToObject<List<string>>() : new List<string>();
                    Dictionary<string, string[]> synonyms = (inRecord.Data.ContainsKey("synonyms")) ? 
                    ((JContainer)inRecord.Data["synonyms"]).ToObject<Dictionary<string, string[]>>() : new Dictionary<string, string[]>();
                    IList<string> exactMatches = (inRecord.Data.ContainsKey("exactMatches")) ? 
                    ((JArray)inRecord.Data["exactMatches"]).ToObject<List<string>>() : new List<string>();
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
            leniency = 10;
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
                    entitiesFound.Add(checkMatch);
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
                    // First find the delineating character for prefix addition later on
                    if ((currWordCharIndex == 0 || currMismatch > 0) && textCharArray[currTextCharIndex].IsDiatric())
                        prevWhiteSpaceIndex = currTextCharIndex;
                    // Skip past extra delineating characters in the front of the word in the text
                    if (currWordCharIndex == 0 && currMismatch >= 0 && textCharArray[currTextCharIndex].IsDiatric())
                    {
                        currTextCharIndex++;
                        wordFound.GetStringBuilder().Clear();
                        currMismatch = 0;
                        continue;
                    }

                    if (currMismatch <= leniency)
                    {
                        if (wordCharArray[currWordCharIndex] == textCharArray[currTextCharIndex])
                        {
                            if (textCharArray[currTextCharIndex].IsDiatric())
                            {
                                if (bestMismatchPreWhitespace == -1 || bestMismatchPreWhitespace > currMismatch + (wordCharArray.Count - currWordCharIndex))
                                {
                                    whitespaceOffset = currTextCharIndex;
                                    bestMismatchPreWhitespace = currMismatch + (wordCharArray.Count - currWordCharIndex);
                                }
                            }
                            wordFound.Write(text.ElementAt(currTextCharIndex));
                            currTextCharIndex++;
                            currWordCharIndex++;
                        }
                        else
                        {
                            double potTextMismatch = (textCharArray[currTextCharIndex].IsAccent()) ? 0.5 : 0;
                            double potWordMismatch = (wordCharArray[currWordCharIndex].IsAccent()) ? 0.5 : 0;
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
                                        if (String.Compare(wordCharArray[currWordCharIndex].ToString(), textCharArray[i].ToString(), CultureInfo.CurrentCulture,
                                             CompareOptions.IgnoreNonSpace) == 0)
                                        {
                                            trueTextComp = true;
                                            if (textCharArray[i - 1].IsDiatric())
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

                                if (trueTextComp && trueWordComp)
                                {
                                    if (potWordMismatch == potTextMismatch)
                                    {
                                        currMismatch += potWordMismatch;
                                        wordFound.Write(text.ElementAt(currTextCharIndex));
                                        currTextCharIndex += offsetWord;
                                        currWordCharIndex += offsetWord;
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
                        // Determine if there is a prefix case
                        
                        if (((currTextCharIndex >= textCharArray.Count && currMismatch + (wordCharArray.Count - currWordCharIndex) <= leniency)
                            || currWordCharIndex >= wordCharArray.Count))
                        {
                            int initialOffsetIndex = currTextCharIndex - wordFound.ToString().Length;
                            int finalOffsetIndex = (initialOffsetIndex - prevWhiteSpaceIndex - 1 <= 0) ? initialOffsetIndex : prevWhiteSpaceIndex + 1;
                            currMismatch += (initialOffsetIndex - prevWhiteSpaceIndex - 1 <= 0) ? 0 : initialOffsetIndex - prevWhiteSpaceIndex - 1;
                            // Determine if there is a suffix case
                            if (currWordCharIndex >= wordCharArray.Count && currTextCharIndex < textCharArray.Count
                                && Char.IsLetterOrDigit(wordCharArray.Last<char>()))
                            {
                                if (!textCharArray[currTextCharIndex - 1].IsDiatric())
                                {
                                    while (currTextCharIndex < textCharArray.Count)
                                    {
                                        if (Char.IsLetterOrDigit(textCharArray[currTextCharIndex]))
                                        {
                                            wordFound.Write(textCharArray[currTextCharIndex]);
                                            currTextCharIndex++;
                                            currMismatch++;
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
                            if (currMismatch <= leniency)
                            {
                                if (bestMismatchPreWhitespace > -1 && bestMismatchPreWhitespace < currMismatch)
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
                                        Confidence = leniency - currMismatch - (wordCharArray.Count - currWordCharIndex)
                                    });

                                    if (bestMismatchPreWhitespace > -1 && whitespaceOffset != initialOffsetIndex)
                                    {
                                        currTextCharIndex = whitespaceOffset;
                                    }
                                }
                                
                                if (!entitiesFound.Contains(checkMatch))
                                    entitiesFound.Add(checkMatch);
                                currWordCharIndex = 0;
                                wordFound.GetStringBuilder().Clear();
                                currMismatch = 0;
                                bestMismatchPreWhitespace = -1;
                            }
                            else
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
        }

        public static IList<char> CreateWordArray(string checkMatch)
        {
            int initCheckIndex = 0;
            int endCheckIndex = checkMatch.Length - 1;

            IList<char> wordCharArray = checkMatch.ToCharArray();

            while (initCheckIndex < checkMatch.Length && wordCharArray[initCheckIndex].IsDiatric())
                initCheckIndex++;
            while (endCheckIndex >= 0 && wordCharArray[endCheckIndex].IsDiatric())
                endCheckIndex--;
            if (initCheckIndex != 0 || endCheckIndex != checkMatch.Length - 1)
            {
                wordCharArray = wordCharArray.ToList<char>().GetRange(initCheckIndex, endCheckIndex + 1);
            }

            return wordCharArray;
        }
        public static bool IsDiatric(this char checkSymbol)
        {
            return (Char.IsWhiteSpace(checkSymbol) || Char.IsSeparator(checkSymbol) || Char.IsPunctuation(checkSymbol));
        }
        public static bool IsAccent(this char checkSymbol)
        {
            return Char.GetUnicodeCategory(checkSymbol) == UnicodeCategory.NonSpacingMark || Char.GetUnicodeCategory(checkSymbol) == UnicodeCategory.SpacingCombiningMark;
        }
    }

}