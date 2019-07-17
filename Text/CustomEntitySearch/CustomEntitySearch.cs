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

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    /// <summary>
    /// Based on sample custom skill provided in Azure Search. Provided a user-defined list of entities
    /// this function determines the entities first occurrence within a given document. This list of entities
    /// must repeatedly be provided by the user for each document.
    /// </summary>
    public static class CustomEntitySearch
    {
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

            const int MAXTIME = 1;
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    string text = inRecord.Data["text"] as string;
                    List<string> words = ((JArray)inRecord.Data["words"]).ToObject<List<string>>();

                    List<Entities> data = new List<Entities>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        foreach (string word in words)
                        {
                            if (string.IsNullOrEmpty(word)) continue;
                            string escapedWord = Regex.Escape(word);
                            string pattern = @"\b(?ix:" + escapedWord + ")";
                            Match entityMatch = Regex.Match(text, pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(MAXTIME));
                            data.Add(
                                new Entities
                                {
                                    Name = word,
                                    MatchIndex = entityMatch.Success ? entityMatch.Index : -1
                                }) ;
                        }
                    }

                    outRecord.Data["entities"] = data;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

    }
}