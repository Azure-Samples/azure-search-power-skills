// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public static class BingEntitySearch
    {
        private static readonly string bingApiEndpoint = "https://api.cognitive.microsoft.com/bing/v7.0/entities/";
        private static readonly string bingApiKeySetting = "BING_API_KEY";

        private static readonly string[] entityTypes = new[] { "Person", "Organization", "Location" };

        [FunctionName("entity-search")]
        public static async Task<IActionResult> RunEntitySearch(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Entity Search Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            string bingApiKey = Environment.GetEnvironmentVariable(bingApiKeySetting, EnvironmentVariableTarget.Process);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var entityName = inRecord.Data["name"] as string;
                    string uri = bingApiEndpoint + "?q=" + Uri.EscapeDataString(entityName) + "&mkt=en-us&count=10&offset=0&safesearch=Moderate";

                    IEnumerable<BingEntity> entities =
                        await WebApiSkillHelpers.FetchAsync<BingEntity>(uri, "Ocp-Apim-Subscription-Key", bingApiKey, "entities.value");

                    ExtractTopEntityMetadata(entities, outRecord.Data);
                    outRecord.Data["entities"] = entities;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        static void ExtractTopEntityMetadata(IEnumerable<BingEntity> entities, Dictionary<string, object> target)
        {
            BingEntity entity = entities?.FirstOrDefault(e =>
                entityTypes.Contains(e.EntityPresentationInfo?.EntityTypeHints?[0])
                && !string.IsNullOrEmpty(e.Description));

            if (entity != null)
            {
                target["description"] = entity.Description;
                target["name"] = entity.Name;
                if (entity.Image != null)
                {
                    target["imageUrl"] = entity.Image.ThumbnailUrl;
                }

                if (entity.ContractualRules != null)
                {
                    foreach (ContractualRule rule in entity.ContractualRules)
                    {
                        if (rule.TargetPropertyName == "description" && rule._type == "ContractualRules/LinkAttribution")
                        {
                            target["url"] = rule.Url;
                        }

                        if (rule._type == "ContractualRules/LicenseAttribution")
                        {
                            target["licenseAttribution"] = rule.LicenseNotice;
                        }
                    }
                }
            }
        }
    }
}
