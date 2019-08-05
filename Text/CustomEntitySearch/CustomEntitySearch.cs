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
        /// 1. All text files are Latin based (i.e. no languageCode)
        /// 2. Words can contain special characters and numbers
        /// 3. The provided entities are not case sensitive
        /// </summary>

        [FunctionName("custom-search")]
        public static async Task<IActionResult> Run(
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
                    string text = inRecord.Data["text"] as string;
                    IList<string> words = ((JArray)inRecord.Data["words"]).ToObject<List<string>>();
                    Dictionary<string, string[]> synonyms = (inRecord.Data.ContainsKey("synonyms")) ? ((JContainer)inRecord.Data["synonyms"]).ToObject<Dictionary<string, string[]>>() : new Dictionary<string, string[]>();
                    IList<string> exactMatches = (inRecord.Data.ContainsKey("exactMatches")) ? ((JArray)inRecord.Data["exactMatches"]).ToObject<List<string>>() : new List<string>();
                    int offset = (inRecord.Data.ContainsKey("fuzzyMatchOffset") && (int)inRecord.Data["fuzzyMatchOffset"] >= 0) ? (int)inRecord.Data["fuzzyMatchOffset"] : 0;
                    if (words.Count == 1 && words[0] == "")
                    {
                        WordLinker userInput = new WordLinker(executionContext.FunctionAppDirectory);
                        words = userInput.Words;
                        synonyms = userInput.Synonyms;
                        exactMatches = userInput.ExactMatch;
                        offset = (userInput.FuzzyMatchOffset >= 0) ? userInput.FuzzyMatchOffset : 0;
                    }
                    
                    var entities = new List<Entity>();
                    var entitiesFound = new HashSet<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        foreach (string word in words)
                        {
                            if (string.IsNullOrEmpty(word)) continue;
                            int leniency = (exactMatches != null && exactMatches.Contains(word)) ? 0 : offset;
                            AddValues(word, text, entities, entitiesFound, leniency);
                            if (synonyms != null && synonyms.ContainsKey(word))
                            {
                                foreach (string synonym in synonyms[word])
                                 {
                                     leniency = (exactMatches != null && exactMatches.Contains(synonym)) ? 0 : offset;
                                     AddValues(synonym, text, entities, entitiesFound, leniency);
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

        private static void AddValues(string checkMatch, string text, List<Entity> entities, HashSet<string> entitiesFound, int leniency)
        {
            bool addWord = false;
            string escapedWord = Regex.Escape(checkMatch);
            string pattern = @"\b(?ix:" + escapedWord + @")\b";
            MatchCollection entityMatch = Regex.Matches(text, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(MaxRegexEvalTime));
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
                        while (cmd >= 0 && wordCharArray[pos] != wordCharArray[cmd])
                        {
                            cmd = KMPTable[cmd];
                        }
                    }
                    pos++;
                    cmd++;
                }
                KMPTable[pos] = cmd;

                // Begin searching!
                int currTextChar = 0;
                int currWordChar = 0;
                int offset = 0;
                StringWriter wordFound = new StringWriter();
                double currMismatch = 0;
                IList<char> textCharArray = text.ToCharArray();

                while (currTextChar < textCharArray.Count)
                {
                    if(wordCharArray[currWordChar] == textCharArray[currTextChar])
                    {
                        wordFound.Write(textCharArray[currTextChar]);
                        currTextChar++;
                        currWordChar++;

                        if (currWordChar >= wordCharArray.Count)
                        {
                            if (currMismatch < leniency && offset > 0)
                            {
                                entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = wordFound.ToString(),
                                        Offset = currTextChar - (currWordChar + offset),
                                        Confidence = (currMismatch / leniency)
                                    });
                                addWord = true;
                                currWordChar = KMPTable[currWordChar];
                                wordFound.Flush();
                                currMismatch = 0;
                                offset = 0;
                            }
                            else
                            {
                                currWordChar = wordCharArray.Count - 1;
                            }
                        }
                    }
                    else
                    {
                        // fuzzy situation?
                        if (currMismatch < leniency)
                        {
                            string accents = @"[\p{Mn}\p{Mc}]";
                            currMismatch += Regex.IsMatch(wordCharArray[currWordChar].ToString(), accents) ? 0.5 : 1;
                            offset++;

                            wordFound.Write(textCharArray[currTextChar]);
                            currTextChar++;
                            currWordChar++;

                            if (currWordChar >= wordCharArray.Count && currMismatch < leniency)
                            {
                                addWord = true;
                                entities.Add(
                                    new Entity
                                    {
                                        Category = "customEntity",
                                        Value = wordFound.ToString(),
                                        Offset = currTextChar - (currWordChar + offset),
                                        Confidence = (currMismatch / leniency)
                                    });
                                currWordChar = KMPTable[currWordChar];
                                wordFound.Flush();
                                currMismatch = 0;
                                offset = 0;
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
                            wordFound.Flush();
                            currMismatch = 0;
                            offset = 0;
                        }

                    }
                }
            }
            if (addWord)
                entitiesFound.Add(checkMatch);
        }
    }
}