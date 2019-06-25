using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Common
{
    public static class WebApiSkillHelpers
    {
        public static IEnumerable<WebApiRequestRecord> GetRequestRecords(HttpRequest req)
        {
            string jsonRequest = new StreamReader(req.Body).ReadToEnd();
            WebApiSkillRequest docs = JsonConvert.DeserializeObject<WebApiSkillRequest>(jsonRequest);
            return docs.Values;
        }

        public static WebApiSkillResponse ProcessRequestRecords(string functionName, IEnumerable<WebApiRequestRecord> requestRecords, Func<WebApiRequestRecord, WebApiResponseRecord, WebApiResponseRecord> processRecord)
        {
            WebApiSkillResponse response = new WebApiSkillResponse();

            foreach (WebApiRequestRecord inRecord in requestRecords)
            {
                WebApiResponseRecord outRecord = new WebApiResponseRecord() { RecordId = inRecord.RecordId };

                try
                {
                    outRecord = processRecord(inRecord, outRecord);
                }
                catch (Exception e)
                {
                    outRecord.Errors.Add(new WebApiErrorWarningContract() { Message = $"{functionName} - Error processing the request record : {e.ToString() }" });
                }
                response.Values.Add(outRecord);
            }

            return response;
        }

        public static async Task<WebApiSkillResponse> ProcessRequestRecordsAsync(string functionName, IEnumerable<WebApiRequestRecord> requestRecords, Func<WebApiRequestRecord, WebApiResponseRecord, Task<WebApiResponseRecord>> processRecord)
        {
            WebApiSkillResponse response = new WebApiSkillResponse();

            foreach (WebApiRequestRecord inRecord in requestRecords)
            {
                WebApiResponseRecord outRecord = new WebApiResponseRecord() { RecordId = inRecord.RecordId };

                try
                {
                    outRecord = await processRecord(inRecord, outRecord);
                }
                catch (Exception e)
                {
                    outRecord.Errors.Add(new WebApiErrorWarningContract() { Message = $"{functionName} - Error processing the request record : {e.ToString() }" });
                }
                response.Values.Add(outRecord);
            }

            return response;
        }

        public static async Task<IEnumerable<T>> Fetch<T>(string uri, string apiKeyHeader, string apiKey, string collectioPath)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add(apiKeyHeader, apiKey);

                HttpResponseMessage response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject responseObject = JObject.Parse(responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"The remote service {uri} responded with a {response.StatusCode} error code: {responseObject["message"]?.ToObject<string>()}");
                }

                if (responseObject == null || !(responseObject.SelectToken(collectioPath) is JToken resultsToken))
                {
                    return Array.Empty<T>();
                }
                return resultsToken
                    .Children()
                    .Select(token => token.ToObject<T>())
                    .ToList();
            }
        }

    }
}