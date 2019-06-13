using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Geo
{
    public static class GeoPointFromName
    {
        private static readonly string azureMapsUri = "https://atlas.microsoft.com/search/fuzzy/json";
        private static readonly string azureMapsKeySetting = "AZUREMAPS_APP_KEY";

        #region input and output records
        private class InputRecord
        {
            public class InputRecordData
            {
                public string Address { get; set; }
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
            public class Position
            {
                public string Lat { get; set; }
                public string Lon { get; set; }
            }

            public class EdmGeographPoint
            {
                public EdmGeographPoint(double lat, double lon)
                {
                    Coordinates = new[] { lon, lat };
                }

                public string Type = "Point";
                public double[] Coordinates { get; set; }
            }

            public class Geography
            {
                public string Type { get; set; }
                public string Score { get; set; }
                public Position Position { get; set; }
            }

            public class OutputRecordData
            {
                public List<Geography> Results { get; set; }
                public EdmGeographPoint MainGeoPoint { get; set; }
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
            public List<OutputRecord> Values { get; set; } = new List<OutputRecord>();
        }
        #endregion

        [FunctionName("GetGeoPointFromName")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Custom skill: C# HTTP trigger function processed a request.");

            // Read input, deserialize it and validate it.
            WebApiRequest data = GetStructuredInput(req.Body);
            if (data == null)
            {
                return new BadRequestObjectResult("The request schema does not match expected schema.");
            }

            string azureMapsKey = Environment.GetEnvironmentVariable(azureMapsKeySetting, EnvironmentVariableTarget.Process);
            // Calculate the response for each value.
            var response = new WebApiResponse();
            foreach (InputRecord record in data.Values)
            {
                if (record == null || record.RecordId == null) continue;

                var responseRecord = new OutputRecord
                {
                    RecordId = record.RecordId
                };

                try
                {
                    responseRecord.Data = await GetPosition(azureMapsKey, record.Data);


                    if (responseRecord.Data != null && responseRecord.Data.Results != null && responseRecord.Data.Results.Count > 0)
                    {

                        var firstPoint = responseRecord.Data.Results[0];

                        if (firstPoint.Position != null)
                        {
                            responseRecord.Data.MainGeoPoint = new OutputRecord.EdmGeographPoint(
                                Convert.ToDouble(firstPoint.Position.Lat),
                                Convert.ToDouble(firstPoint.Position.Lon));
                        }
                    }

                }
                catch (Exception e)
                {
                    // Something bad happened, log the issue.
                    var error = new OutputRecord.OutputRecordMessage
                    {
                        Message = e.Message
                    };

                    responseRecord.Errors = new List<OutputRecord.OutputRecordMessage>
                    {
                        error
                    };
                }
                finally
                {
                    response.Values.Add(responseRecord);
                }
            }

            return new OkObjectResult(response);
        }

        private static WebApiRequest GetStructuredInput(Stream requestBody)
            => JsonConvert.DeserializeObject<WebApiRequest>(new StreamReader(requestBody).ReadToEnd());

        /// <summary>
        /// Use Azure Maps to find location of an address
        /// </summary>
        /// <param name="address">The address to search for.</param>
        /// <returns>Asynchronous task that returns objects identified in the image. </returns>
        async static Task<OutputRecord.OutputRecordData> GetPosition(string azureMapsKey, InputRecord.InputRecordData inputRecord)
        {
            var result = new OutputRecord.OutputRecordData();

            var uri = azureMapsUri + "?api-version=1.0&query=" + Uri.EscapeDataString(inputRecord.Address);

            try
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Get;
                    request.RequestUri = new Uri(uri);
                    request.Headers.Add("X-ms-client-id", azureMapsKey);

                    var response = await client.SendAsync(request);
                    var responseBody = await response.Content.ReadAsStringAsync();

                    result = JsonConvert.DeserializeObject<OutputRecord.OutputRecordData>(responseBody);
                }
            }
            catch
            {
                result = new OutputRecord.OutputRecordData();
            }

            return result;
        }
    }
}
