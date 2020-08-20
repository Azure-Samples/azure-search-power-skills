using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CustomVision
{
    public static class CustomVision
    {

        private static readonly string customVisionPredictionUrlSetting = "CUSTOM_VISION_PREDICTION_URL";
        private static readonly string customVisionApiKeySetting = "CUSTOM_VISION_API_KEY";
        private static readonly string maxPagesSetting = "MAX_PAGES";
        private static readonly string minProbabilityThresholdSetting = "MIN_PROBABILITY_THRESHOLD";

        [FunctionName("custom-vision")]
        public static async Task<IActionResult> RunCustomVision(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Custom Vision Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }


            var predictionUrl = Environment.GetEnvironmentVariable(customVisionPredictionUrlSetting, EnvironmentVariableTarget.Process);
            var predictionKey = Environment.GetEnvironmentVariable(customVisionApiKeySetting, EnvironmentVariableTarget.Process);
            var maxPages = Convert.ToInt32(Environment.GetEnvironmentVariable(maxPagesSetting, EnvironmentVariableTarget.Process));
            if (maxPages == 0) 
                maxPages = 1;
            var threshold = Convert.ToDouble(Environment.GetEnvironmentVariable(minProbabilityThresholdSetting, EnvironmentVariableTarget.Process));
            if (threshold == 0.0)
                threshold = 0.5;

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var pages = inRecord.Data["pages"] as JArray;
                    var tags = new List<string>();
                    var predictions = new List<object>();
                    foreach (var page in pages.Take(maxPages))
                    {
                        var pageBinaryData = Convert.FromBase64String(page.ToString());
                        var pagePredictions = await GetPredictionsForImageAsync(pageBinaryData, predictionUrl, predictionKey);

                        var analyzeResult = JsonConvert.DeserializeObject<AnalyzeResult>(pagePredictions);
                        var pageTags = analyzeResult.Predictions.Where(p => p.Probability >= threshold).Select(p => p.TagName).Distinct();
                        tags.AddRange(pageTags);
                    }
                    
                    outRecord.Data["tags"] = tags.Distinct().ToArray();
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        public static async Task<string> GetPredictionsForImageAsync(byte[] imageData, string predictionUrl, string predictionKey)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);

            using (var content = new ByteArrayContent(imageData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await client.PostAsync(predictionUrl, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        class Prediction
        {
            public double Probability { get; set; }
            public string TagName { get; set; }

            public override string ToString()
            {
                return $"tag: {TagName}, probability {Probability}";
            }
        }

        class AnalyzeResult
        {
            public Prediction[] Predictions { get; set; }
        }
    }
}
