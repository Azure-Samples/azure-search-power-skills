using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using Azure.AI.TextAnalytics;
using Azure;

namespace HealthcareTA
{
    public static class HealthcareTA
    {
        private static readonly string healthcareApiEnvEndpoint = "HEALTHCARE_API_ENDPOINT";
        private static readonly string healthcareApiEnvKey = "HEALTHCARE_API_KEY";

        [FunctionName("HealthcareTA")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            string apiKey = Environment.GetEnvironmentVariable(healthcareApiEnvKey, EnvironmentVariableTarget.Process);
            string apiEndpoint = Environment.GetEnvironmentVariable(healthcareApiEnvEndpoint, EnvironmentVariableTarget.Process);
            if (apiKey == null || apiEndpoint == null)
            {
                return new BadRequestObjectResult($"{skillName} - Healthcare Text Analytics API key or endpoint is missing. Make sure to set them in the Environment Variables.");
            }
            // "abd52a281bc64a8db44c9e31d8f4aa39";
            var client = new TextAnalyticsClient(new Uri(apiEndpoint), new AzureKeyCredential(apiKey));


            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var document = inRecord.Data["document"] as string;

                    // prepare analyze operation input
                    List<string> batchInput = new List<string>()
                    {
                        document
                    };
                    var options = new AnalyzeHealthcareEntitiesOptions { };

                    // start analysis process
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    AnalyzeHealthcareEntitiesOperation healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(batchInput, "en", options);
                    await healthOperation.WaitForCompletionAsync();
                    await ExtractEntityData(healthOperation.Value, outRecord);
                    timer.Stop();

                    outRecord.Data["status"] = healthOperation.Status.ToString();
                    outRecord.Data["timeToComplete"] = timer.Elapsed.TotalSeconds.ToString();

                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        private static async Task ExtractEntityData(AsyncPageable<AnalyzeHealthcareEntitiesResultCollection> pages, WebApiResponseRecord outRecord)
        {
            await foreach (AnalyzeHealthcareEntitiesResultCollection documentsInPage in pages)
            {
                foreach (AnalyzeHealthcareEntitiesResult entitiesInDoc in documentsInPage)
                {
                    if (!entitiesInDoc.HasError)
                    {
                        outRecord.Data["entities"] = entitiesInDoc.Entities;
                        outRecord.Data["relations"] = entitiesInDoc.EntityRelations;
                    }
                    else
                    {
                        var newError = new WebApiErrorWarningContract();
                        newError.Message = $"Healthcare Text Analytics Error: {entitiesInDoc.Error.ErrorCode}. Error Message: {entitiesInDoc.Error.Message}";
                        outRecord.Errors.Add(newError);
                    }
                }
            }
        }
    }
}
