// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
using System.Globalization;

namespace AzureCognitiveSearch.PowerSkills.Text.TextAnalyticsForHealth
{
    public static class TextAnalyticsForHealth
    {
        public static readonly string textAnalyticsApiEndpointSetting = "TEXT_ANALYTICS_API_ENDPOINT";
        public static readonly string textAnalyticsApiKeySetting = "TEXT_ANALYTICS_API_KEY";
        private static readonly int defaultTimeout = 230;
        private static readonly int maxTimeout = 230;
        private static readonly int timeoutBuffer = 5;
        private static readonly int maxCharLength = 5000;

        [FunctionName("TextAnalyticsForHealth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            // Get Endpoint and access key from App Settings
            string apiKey = Environment.GetEnvironmentVariable(textAnalyticsApiKeySetting, EnvironmentVariableTarget.Process);
            string apiEndpoint = Environment.GetEnvironmentVariable(textAnalyticsApiEndpointSetting, EnvironmentVariableTarget.Process);
            if (apiKey == null || apiEndpoint == null)
            {
                return new BadRequestObjectResult($"{skillName} - TextAnalyticsForHealth API key or Endpoint is missing. Make sure to set it in the Environment Variables.");
            }
            var client = new TextAnalyticsClient(new Uri(apiEndpoint), new AzureKeyCredential(apiKey));

            // Get a custom timeout from the header, if it exists. If not use the default timeout.
            int timeout;
            if (!int.TryParse(req.Headers["timeout"].ToString(), out timeout))
            {
                timeout = defaultTimeout;
            }
            timeout = Math.Clamp(timeout - timeoutBuffer, 1, maxTimeout - timeoutBuffer);
            var timeoutMiliseconds = timeout * 1000;
            var timeoutTask = Task.Delay(timeoutMiliseconds);

            // Get a custom default language, if none is provided, use english
            string defaultLanguage = req.Headers.ContainsKey("defaultLanguageCode")? req.Headers["defaultLanguageCode"].ToString(): "en";

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    if (timeoutTask.IsCompleted)
                    {
                        // The time limit for all the skills has been met
                        outRecord.Errors.Add(new WebApiErrorWarningContract
                        {
                            Message = "TextAnalyticsForHealth Error: The Text Analysis Operation took too long to complete."
                        });
                        return outRecord;
                    }

                    // Prepare analysis operation input
                    if (!inRecord.Data.ContainsKey("text"))
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract
                        {
                            Message = "TextAnalyticsForHealth Error: The skill request did not contain 'text' in the input."
                        });
                        return outRecord;
                    }
                    var document = inRecord.Data["text"] as string;
                    var language = inRecord.Data.ContainsKey("languageCode") ? inRecord.Data["languageCode"] as string : defaultLanguage;

                    var docInfo = new StringInfo(document);
                    if (docInfo.LengthInTextElements >= maxCharLength)
                    {
                        outRecord.Warnings.Add(new WebApiErrorWarningContract
                        {
                            Message = $"TextAnalyticsForHealth Warning: The submitted document was over {maxCharLength} elements. It has been truncated to fit this requirement."
                        });
                        document = docInfo.SubstringByTextElements(0, maxCharLength);
                    }

                    var options = new AnalyzeHealthcareEntitiesOptions { };
                    List<string> batchInput = new List<string>()
                    {
                        document
                    };

                    // start analysis process
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    AnalyzeHealthcareEntitiesOperation healthOperation = await client.StartAnalyzeHealthcareEntitiesAsync(batchInput, language, options);
                    var healthOperationTask = healthOperation.WaitForCompletionAsync().AsTask();

                    if (await Task.WhenAny(healthOperationTask, timeoutTask) == healthOperationTask)
                    {
                        // Task Completed, now lets process the result.
                        outRecord.Data["status"] = healthOperation.Status.ToString();
                        if (healthOperation.Status != TextAnalyticsOperationStatus.Succeeded || !healthOperation.HasValue)
                        {
                            // The operation was not a success
                            outRecord.Errors.Add(new WebApiErrorWarningContract
                            {
                                Message = "TextAnalyticsForHealth Error: Health Operation returned a non-succeeded status."
                            });
                        }
                        else
                        {
                            // The operation was a success, so lets add the results to our output.
                            await ExtractEntityData(healthOperation.Value, outRecord);
                        }
                    }
                    else
                    {
                        // Timeout
                        outRecord.Errors.Add(new WebApiErrorWarningContract
                        {
                            Message = "TextAnalyticsForHealth Error: The Text Analysis Operation took too long to complete."
                        });
                    }

                    // Record how long this task took to complete.
                    timer.Stop();
                    var timeToComplete =  timer.Elapsed.TotalSeconds;
                    log.LogInformation($"Time to complete request for document with ID {inRecord.RecordId}: {timeToComplete}");

                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        private static async Task ExtractEntityData(AsyncPageable<AnalyzeHealthcareEntitiesResultCollection> pages, WebApiResponseRecord outRecord)
        {
            // Based on our input, there should only be one page per pages, and one document per page, but to guarantuee success we collect
            // all output into these two collections.
            var entities = new List<Object>();
            var relations = new List<Object>();
            await foreach (AnalyzeHealthcareEntitiesResultCollection page in pages)
            {
                foreach (AnalyzeHealthcareEntitiesResult document in page)
                {
                    if (!document.HasError)
                    {
                        entities.AddRange(document.Entities);
                        relations.AddRange(document.EntityRelations);
                    }
                    else
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract{
                            Message = $"TextAnalyticsForHealth Error: {document.Error.ErrorCode}. Error Message: {document.Error.Message}"
                        });
                    }

                    if (document.Warnings.Count > 0)
                    {
                        foreach (TextAnalyticsWarning w in document.Warnings)
                        {
                            outRecord.Warnings.Add(new WebApiErrorWarningContract
                            {
                                Message = $"TextAnalyticsForHealth Warning: {w.WarningCode}. Error Message: {w.Message}"
                            });
                        }
                    }
                }
            }
            outRecord.Data[$"entities"] = entities;
            outRecord.Data[$"relations"] = relations;
        }
    }
}
