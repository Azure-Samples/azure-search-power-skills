// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.AcronymLinker
{
    public static class LinkAcronyms
    {
        [FunctionName("link-acronyms")]
        public static IActionResult RunAcronymLinker(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Link Acronyms Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            AcronymLinker acronymLinker = new AcronymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    string word = inRecord.Data["word"] as string;
                    if (word.All(char.IsUpper) && acronymLinker.Acronyms.TryGetValue(word, out string description))
                    {
                        outRecord.Data["acronym"] = new { value = word, description };
                    }
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        [FunctionName("link-acronyms-list")]
        public static IActionResult RunAcronymLinkerForLists([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Link Acronyms List Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            AcronymLinker acronymLinker = new AcronymLinker(executionContext.FunctionAppDirectory);
            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    var words = JsonConvert.DeserializeObject<JArray>(JsonConvert.SerializeObject(inRecord.Data["words"]));
                    var acronyms = words
                        .Distinct()
                        .Select(jword =>
                        {
                            var word = jword.Value<string>();
                            if (word.All(char.IsUpper) && acronymLinker.Acronyms.TryGetValue(word, out string description))
                            {
                                return new { value = word, description };
                            }
                            return null;
                        })
                        .Where(acronym => acronym != null);

                    outRecord.Data["acronyms"] = acronyms.ToArray();
                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
