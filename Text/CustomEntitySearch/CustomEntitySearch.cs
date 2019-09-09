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
        public static IList<string> preLoadedWords = new WordLinker("csv").Words;

        private static readonly int MaxRegexEvalTime = 1;
        /// <summary>
        /// We assert the following assumptions:
        /// 1. All text files are Latin based (i.e. no languageCode)
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
                    string text = inRecord.Data["text"] as string;

                    IList<string> words = preLoadedWords;

                    // Check if user supplied their own words
                    if (inRecord.Data.ContainsKey("words") == true)
                    {
                        words = ((JArray)inRecord.Data["words"]).ToObject<List<string>>();
                    }
                    
                    var entities = new HashSet<Entity>();
                    var entitiesFound = new HashSet<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        foreach (string word in words)
                        {
                            if (string.IsNullOrEmpty(word)) continue;
                            string escapedWord = Regex.Escape(word);
                            string pattern = @"\b(?ix:" + escapedWord + @")\b";
                            MatchCollection entityMatch = Regex.Matches(text, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(MaxRegexEvalTime));
                            if (entityMatch.Count != 0)
                            {
                                foreach (Match match in entityMatch)
                                {
                                    entities.Add(
                                        new Entity
                                        {
                                            Name = match.Value,
                                            MatchIndex = match.Index
                                        });
                                }
                                entitiesFound.Add(word);
                            }

                        }
                    }

                    outRecord.Data["Entities"] = entities;
                    outRecord.Data["EntitiesFound"] = entitiesFound;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

    }
}