using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;

namespace AzureCognitiveSearch.PowerSkills.Vision.AnalyzeForm
{
    public static class AnalyzeForm
    {
        private static readonly string formsRecognizerApiEndpoint =
            "https://westus2.api.cognitive.microsoft.com/formrecognizer/v1.0-preview/custom";
        private static readonly string formsRecognizerApiKeySetting = "FORMS_RECOGNIZER_API_KEY";
        private static readonly string modelIdSetting = "FORMS_RECOGNIZER_MODEL_ID";

        // Modify this list of fields to extract according to your requirements:
        private static readonly Dictionary<string, string> fieldMappings = new Dictionary<string, string> {
            { "Address:", "address" },
            { "Invoice For:", "recipient" }
        };
        // Modify this content type if you need to handle file types other than PDF
        private static readonly string contentType = "application/pdf";

        [FunctionName("AnalyzeForm")]
        public static async Task<IActionResult> RunAnalyzeForm(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Analyze Form Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            string formsRecognizerApiKey = Environment.GetEnvironmentVariable(formsRecognizerApiKeySetting, EnvironmentVariableTarget.Process);
            string modelId = Environment.GetEnvironmentVariable(modelIdSetting, EnvironmentVariableTarget.Process);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var formUrl = inRecord.Data["formUrl"] as string;
                    var formSasToken = inRecord.Data["formSasToken"] as string;

                    // Fetch the document
                    byte[] documentBytes;
                    using (var webClient = new WebClient())
                    {
                        documentBytes = await webClient.DownloadDataTaskAsync(new Uri(formUrl + formSasToken));
                    }

                    string uri = formsRecognizerApiEndpoint + "/models/" + Uri.EscapeDataString(modelId) + "/analyze";

                    List<Page> pages =
                        (await WebApiSkillHelpers.FetchAsync<Page>(
                            uri,
                            apiKeyHeader: "Ocp-Apim-Subscription-Key",
                            apiKey: formsRecognizerApiKey,
                            collectionPath: "pages",
                            method: HttpMethod.Post,
                            postBody: documentBytes,
                            contentType))
                        .ToList();

                    foreach(KeyValuePair<string, string> kvp in fieldMappings)
                    {
                        var value = GetField(pages, kvp.Key);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            outRecord.Data[kvp.Value] = value;
                        }
                    }
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Searches for a field in a given page and returns the concatenated results.
        /// </summary>
        /// <param name="response">the responsed from the forms recognizer service.</param>
        /// <param name="fieldName">The field to search for</param>
        /// <returns></returns>
        private static string GetField(IList<Page> pages, string fieldName)
        {
            var value = pages
                .SelectMany(p => p.KeyValuePairs)
                .Where(kvp => kvp.Key
                    .Select(key => key.Text.Trim())
                    .Contains(fieldName, StringComparer.CurrentCultureIgnoreCase)
                )
                .SelectMany(kvp => kvp.Value.Select(v => v.Text));
            return value == null ? null : string.Join(" ", value);
        }
    }
}
