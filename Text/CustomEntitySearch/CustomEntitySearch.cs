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
        /// 1. All text files only characters provided in Unicode Character Database 
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
                     }
                     else if (!inRecord.Data.ContainsKey("words") && 
                     (inRecord.Data.ContainsKey("synonyms") || inRecord.Data.ContainsKey("exactMatches") || inRecord.Data.ContainsKey("fuzzyMatchOffset")))
                     {
                         outRecord.Errors.Add(new WebApiErrorWarningContract { Message = "Cannot process record without the given key 'words' in the dictionary"});
                     }
                     else
                     {
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
                         if (words.Count == 0 || (words.Count == 1 && words[0] == ""))
                         {
                             outRecord.Warnings.Add(new WebApiErrorWarningContract { Message = "The given key 'words' was not present in the dictionary." });
                             WordLinker userInput = new WordLinker(executionContext.FunctionAppDirectory);
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
                                 if (synonyms != null && synonyms.ContainsKey(word))
                                 {
                                     foreach (string synonym in synonyms[word])
                                     {
                                         leniency = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : offset;
                                         AddValues(synonym, text, entities, entitiesFound, leniency, caseSensitive);
                                     }
                                 }
                             }
                         }

                         outRecord.Data["Entities"] = entities;
                         outRecord.Data["EntitiesFound"] = entitiesFound;
                     }
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        public static void AddValues(string checkMatch, string text, List<Entity> entities, HashSet<string> entitiesFound, int leniency, bool caseSensitive)
        {
            bool addWord = false;
            string escapedWord = Regex.Escape(checkMatch);
            string pattern = (caseSensitive) ? @"\b(?x:" + escapedWord + @")\b" : @"\b(?ix:" + escapedWord + @")\b";
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

            if (leniency > 0)
            {
                // using KMP for now
                // Create Table
                IList<int> KMPTable = (caseSensitive) ? CreateKMPTable(checkMatch) : CreateKMPTable(checkMatch.ToLower());

                // Begin searching!
                int currTextChar = 0;
                int currWordChar = 0;
                StringWriter wordFound = new StringWriter();
                double currMismatch = 0;
                IList<char> wordCharArray = (caseSensitive) ? checkMatch.ToCharArray() : checkMatch.ToLower().ToCharArray();
                IList<char> textCharArray = (caseSensitive) ? text.ToCharArray() : text.ToLower().ToCharArray();
                IList<string> valuesList = new List<string>();
                IList<int> offsetList = new List<int>();
                IList<double> confidenceList = new List<double>();
                int minVal = -1;

                while (currTextChar < textCharArray.Count)
                {
                    if (currWordChar == 0 && currMismatch > 0)
                    {
                        wordFound.GetStringBuilder().Remove(0, 1);
                        currMismatch--;
                    }
                    if (currMismatch <= leniency)
                    {
                        if (wordCharArray[currWordChar] == textCharArray[currTextChar])
                        {
                            wordFound.Write(text.ElementAt(currTextChar));
                            currTextChar++;
                            currWordChar++;

                            if (currWordChar >= wordCharArray.Count || (currTextChar >= textCharArray.Count 
                                && currMismatch + (wordCharArray.Count - currWordChar) <= leniency))
                            {
                                // Code Cleanup?
                                if (currMismatch > 0 || entityMatch.Count == 0)
                                {
                                    if (!entityMatch.Where(x => (x.Index >= currTextChar - (wordCharArray.Count + leniency)) &&
                                    (x.Index <= currTextChar - wordCharArray.Count)).Any())
                                    {
                                        valuesList.Add(wordFound.ToString());
                                        offsetList.Add(currTextChar - wordFound.ToString().Length);
                                        confidenceList.Add(currMismatch + (wordCharArray.Count - currWordChar) / leniency);
                                        if (minVal < 0 || wordFound.ToString().Length < minVal)
                                            minVal = wordFound.ToString().Length;
                                        addWord = true;
                                    }
                                    currWordChar = KMPTable[currWordChar];
                                    wordFound.GetStringBuilder().Clear();
                                    currMismatch = 0;
                                }
                                else
                                {
                                    currWordChar = KMPTable[currWordChar];
                                    if (currWordChar < 0)
                                    {
                                        currWordChar++;
                                        currTextChar++;
                                    }
                                    wordFound.GetStringBuilder().Clear();
                                    currMismatch = 0;
                                }
                            }
                        }
                        else
                        {
                            string accents = @"[\p{Mn}\p{Sk}\p{Mc}]";
                            bool wordHalfMark = Regex.Match(wordCharArray[currWordChar].ToString(), accents).Success;
                            bool textHalfMark = Regex.Match(textCharArray[currTextChar].ToString(), accents).Success;

                            // fuzzy situation?
                            // accent case adds 0.5
                            if ((wordHalfMark && textHalfMark) ||
                                String.Compare(wordCharArray[currWordChar].ToString(), textCharArray[currTextChar].ToString(),
                                CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                            {
                                currMismatch += 0.5;
                                wordFound.Write(text.ElementAt(currTextChar));
                                currTextChar++;
                                currWordChar++;
                            }
                            else if (currTextChar < textCharArray.Count - 1 && String.Compare(wordCharArray[currWordChar].ToString(), 
                                textCharArray[currTextChar+1].ToString(), CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                            {
                                currMismatch += (textHalfMark) ? 0.5 : 1;
                                wordFound.Write(text.ElementAt(currTextChar));
                                currTextChar++;
                            }
                            else if (currWordChar < wordCharArray.Count - 1 && String.Compare(wordCharArray[currWordChar + 1].ToString(),
                                textCharArray[currTextChar].ToString(), CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0)
                            {
                                currMismatch += (wordHalfMark) ? 0.5 : 1;
                                currWordChar++;
                            }
                            else
                            {
                                currMismatch += 1;
                                wordFound.Write(text.ElementAt(currTextChar));
                                currTextChar++;
                                currWordChar++;
                            }

                            if((currWordChar >= wordCharArray.Count || (currTextChar >= textCharArray.Count
                                && currMismatch + (wordCharArray.Count - currWordChar) <= leniency))  && currMismatch <= leniency)
                            {
                                if (!entityMatch.Where(x => (x.Index >= currTextChar - (wordCharArray.Count + leniency)) &&
                                    (x.Index <= currTextChar - wordCharArray.Count)).Any())
                                {
                                    addWord = true;
                                    valuesList.Add(wordFound.ToString());
                                    offsetList.Add(currTextChar - wordFound.ToString().Length);
                                    confidenceList.Add(currMismatch + (wordCharArray.Count - currWordChar) / leniency);
                                    if (minVal < 0 || wordFound.ToString().Length < minVal)
                                        minVal = wordFound.ToString().Length;
                                }
                                currWordChar = KMPTable[currWordChar];
                                wordFound.GetStringBuilder().Clear();
                                currMismatch = 0;
                            }
                        }
                    }
                    else
                    {
                        currWordChar = KMPTable[currWordChar];
                        if (currWordChar < 0)
                        {
                            currWordChar++;
                            currTextChar++;
                        }
                        wordFound.GetStringBuilder().Clear();
                        currMismatch = 0;
                    }
                }

                for (int i = 0; i < valuesList.Count; i++)
                {
                    if (valuesList[i].Length == minVal)
                    {
                        entities.Add(
                            new Entity
                            {
                                Category = "customEntity",
                                Value = valuesList[i],
                                Offset = offsetList[i],
                                Confidence = 1 - confidenceList[i]
                            });
                    }
                }
            }
            if (addWord)
                entitiesFound.Add(checkMatch);
        }

        public static IList<int> CreateKMPTable(string checkMatch)
        {
            IList<int> KMPTable = new List<int>(new int[checkMatch.Length + 1]);
            IList<char> wordCharArray = checkMatch.ToCharArray();
            int pos = 1;
            int cmd = 0;
            KMPTable[0] = -1;

            while (pos < checkMatch.Length)
            {
                if (wordCharArray[pos] == wordCharArray[cmd])
                {
                    KMPTable[pos] = KMPTable[cmd];
                }
                else
                {
                    KMPTable[pos] = cmd;
                    cmd = KMPTable[cmd];
                    if (cmd >= 0 && wordCharArray[pos] != wordCharArray[cmd])
                    {
                        cmd = KMPTable[cmd];
                    }
                }
                pos++;
                cmd++;
            }
            KMPTable[pos] = cmd;
            return KMPTable;
        }
    }
}