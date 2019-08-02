using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests
{
    public static class Helpers
    {
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public static async Task<WebApiSkillResponse> QueryFunction(string inputText, Func<HttpRequest, Task<IActionResult>> function)
        {
            var jsonContent = new StringContent(inputText, null, "application/json");
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "application/json; charset=utf-8",
                Body = await jsonContent.ReadAsStreamAsync(),
                Method = "POST"
            };
            var response = (OkObjectResult)(await function(request));
            return (WebApiSkillResponse)response.Value;
        }

        public static async Task<string> QueryFunctionAndSerialize(string inputText, Func<HttpRequest, Task<IActionResult>> function)
            => JsonConvert.SerializeObject(await QueryFunction(inputText, function), _jsonSettings);

        public static string BuildPayload(object data)
            => JsonConvert.SerializeObject(new
            {
                Values = new object[]
                {
                    new
                    {
                        RecordId = "1",
                        Data = data,
                        Errors = Array.Empty<string>(),
                        Warnings = Array.Empty<string>()
                    }
                }
            }, _jsonSettings);
    }
}
