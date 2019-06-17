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
using System.Net.Http;
using System.Threading;

namespace AzureCognitiveSearch.PowerSkills.Text
{
    public static class BingEntitySearch
    {
        private static readonly string bingApiEndpoint = "https://api.cognitive.microsoft.com/bing/v7.0/entities/";
        private static readonly string bingApiKeySetting = "BING_API_KEY";

        #region input and output records
        private class InputRecord
        {
            public class InputRecordData
            {
                public string Name { get; set; }
            }

            public string RecordId { get; set; }
            public InputRecordData Data { get; set; }
        }

        private class WebApiRequest
        {
            public List<InputRecord> Values { get; set; }
        }
        private class OutputRecord
        {
            public class OutputRecordData
            {
                public string Name { get; set; }
                public string Description { get; set; }
                public string ImageUrl { get; set; }
                public string Url { get; set; }
                public string LicenseAttribution { get; set; }
                public Entities Entities { get; set; }
            }

            public class OutputRecordMessage
            {
                public string Message { get; set; }
            }

            public string RecordId { get; set; }
            public OutputRecordData Data { get; set; }
            public List<OutputRecordMessage> Errors { get; set; }
            public List<OutputRecordMessage> Warnings { get; set; }
        }

        private class WebApiResponse
        {
            public List<OutputRecord> Values { get; set; }
        }

        private class Entities
        {
            public BingEntity[] Value { get; set; }
        }

        private class BingEntity
        {
            public class Entitypresentationinfo
            {
                public string EntityScenario { get; set; }
                public string[] EntityTypeHints { get; set; }
                public object EntityTypeDisplayHint { get; set; }
            }

            public class License
            {
                public string Name { get; set; }
                public string Url { get; set; }
            }

            public class Contractualrule
            {
                public string _type { get; set; }
                public string TargetPropertyName { get; set; }
                public bool MustBeCloseToContent { get; set; }
                public License License { get; set; }
                public string LicenseNotice { get; set; }
                public string Text { get; set; }
                public string Url { get; set; }
            }

            public class Provider
            {
                public string _type { get; set; }
                public string Url { get; set; }
            }


            public class ImageClass
            {
                public string Name { get; set; }
                public string ThumbnailUrl { get; set; }
                public Provider[] Provider { get; set; }
                public string HostPageUrl { get; set; }
                public int Width { get; set; }
                public int Height { get; set; }
            }

            public Contractualrule[] contractualRules { get; set; }
            public ImageClass Image { get; set; }
            public string Description { get; set; }
            public string BingId { get; set; }
            public string WebSearchUrl { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public Entitypresentationinfo EntityPresentationInfo { get; set; }
        }
        #endregion

        [FunctionName("BingEntitySearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Entity Search function: C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            WebApiRequest data = JsonConvert.DeserializeObject<WebApiRequest>(requestBody);

            // Do some schema validation
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }
            if (data.Values == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema. Could not find values array.");
            }

            string bingMapsKey = Environment.GetEnvironmentVariable(bingApiKeySetting, EnvironmentVariableTarget.Process);

            var response = new WebApiResponse
            {
                Values = new List<OutputRecord>()
            };

            // Calculate the response for each value.
            foreach (var record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                var responseRecord = new OutputRecord
                {
                    RecordId = record.RecordId
                };

                try
                {
                    responseRecord.Data = GetEntityMetadata(record.Data.Name, bingMapsKey).Result;
                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    responseRecord.Errors = new List<OutputRecord.OutputRecordMessage>
                    {
                        new OutputRecord.OutputRecordMessage
                        {
                            Message = e.Message
                        }
                    };
                }
                finally
                {
                    response.Values.Add(responseRecord);
                }
            }

            return new OkObjectResult(response);
        }

        /// <summary>
        /// Gets metadata for a particular entity based on its name using Bing Entity Search
        /// </summary>
        /// <param name="dataName">The image to extract objects for.</param>
        /// <returns>Asynchronous task that returns objects identified in the image. </returns>
        async static Task<OutputRecord.OutputRecordData> GetEntityMetadata(string dataName, string bingApiKey)
        {
            var result = new OutputRecord.OutputRecordData();

            var uri = bingApiEndpoint + "?q=" + Uri.EscapeDataString(dataName) + "&mkt=en-us&count=10&offset=0&safesearch=Moderate";

            using (var client = new HttpClient(new RetryHandler(new HttpClientHandler())))
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", bingApiKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                result = JsonConvert.DeserializeObject<OutputRecord.OutputRecordData>(responseBody);

                // In addition to the list of entities that could match the name, for simplicity let's return information
                // for the top match as additional metadata at the root object.
                AddTopEntityMetadata(result);

                // Do some cleanup on the returned result.
                result.ImageUrl = result.ImageUrl ?? "";
                result.Description = result.Description ?? "";
                if (result.Name == null)
                {
                    result.Name = dataName ?? "";
                }
                result.Url = result.Url ?? "";
                result.LicenseAttribution = result.LicenseAttribution ?? "";
            }

            return result;
        }

        static void AddTopEntityMetadata(OutputRecord.OutputRecordData rootObject)
        {
            if (rootObject.Entities != null)
            {
                foreach (BingEntity entity in rootObject.Entities.Value)
                {
                    if (entity.EntityPresentationInfo != null
                        && entity.EntityPresentationInfo.EntityTypeHints != null
                        && entity.EntityPresentationInfo.EntityTypeHints[0] != "Person"
                        && entity.EntityPresentationInfo.EntityTypeHints[0] != "Organization"
                        && entity.EntityPresentationInfo.EntityTypeHints[0] != "Location")
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(entity.Description))
                    {
                        rootObject.Description = entity.Description;
                        rootObject.Name = entity.Name;
                        if (entity.Image != null)
                        {
                            rootObject.ImageUrl = entity.Image.ThumbnailUrl;
                        }

                        if (entity.contractualRules != null)
                        {
                            foreach (BingEntity.Contractualrule rule in entity.contractualRules)
                            {
                                if (rule.TargetPropertyName == "description")
                                {
                                    rootObject.Url = rule.Url;
                                }

                                if (rule._type == "ContractualRules/LicenseAttribution")
                                {
                                    rootObject.LicenseAttribution = rule.LicenseNotice;
                                }
                            }
                        }
                    }
                }
            }
        }

        public class RetryHandler : DelegatingHandler
        {
            private const int MaxRetries = 10;

            public RetryHandler(HttpMessageHandler innerHandler)
                : base(innerHandler) { }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                HttpResponseMessage response = null;
                for (int i = 0; i < MaxRetries; i++)
                {
                    response = await base.SendAsync(request, cancellationToken);
                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    Thread.Sleep(1000);
                }

                return response;
            }
        }
    }
}
