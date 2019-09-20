// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net;
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

        public static async Task<object> QuerySkill(
            Func<HttpRequest, ILogger, Microsoft.Azure.WebJobs.ExecutionContext, Task<IActionResult>> skillFunction,
            object payload,
            string outputPath)
        {
            WebApiSkillResponse skillOutput = await QueryFunction(BuildPayload(payload), skillFunction);
            return skillOutput.Values[0].Data.TryGetValue(outputPath, out object output) ? output : null;
        }

        public static async Task<object> QuerySkill(
            Func<HttpRequest, ILogger, Microsoft.Azure.WebJobs.ExecutionContext, IActionResult> skillFunction,
            object payload,
            string outputPath)
            => await QuerySkill(
                (HttpRequest req, ILogger logger, Microsoft.Azure.WebJobs.ExecutionContext ctx)
                    => Task.FromResult(skillFunction(req, logger, ctx)),
                payload, outputPath);

        public static async Task<WebApiSkillResponse> QueryFunction(
            string inputText,
            Func<HttpRequest, ILogger, Microsoft.Azure.WebJobs.ExecutionContext, Task<IActionResult>> function)
            => await QueryFunction(inputText, CurrySkillFunction(function));

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

        public static async Task<string> QueryFunctionAndSerialize(
            string inputText,
            Func<HttpRequest, ILogger, Microsoft.Azure.WebJobs.ExecutionContext, Task<IActionResult>> function)
            => await QueryFunctionAndSerialize(inputText, CurrySkillFunction(function));

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

        public static HttpResponseMessage RespondRequestWith(this HttpRequestMessage req, object responseBody)
        {
            req.Properties.Add(nameof(HttpContext), new DefaultHttpContext
            {
                RequestServices = new ServiceCollection()
                .AddMvc()
                .AddWebApiConventions()
                .Services
                .BuildServiceProvider()
            });
            return req.CreateResponse(HttpStatusCode.OK, responseBody);
        }

        public static Func<HttpRequest, Task<IActionResult>> CurrySkillFunction(Func<HttpRequest, ILogger, Microsoft.Azure.WebJobs.ExecutionContext, Task<IActionResult>> skillFunction)
            => request => skillFunction(request, new LoggerFactory().CreateLogger("local"), new Microsoft.Azure.WebJobs.ExecutionContext());

        public static T GetProperty<T>(this object obj, string propertyName) where T : class
            => obj.GetType().GetProperty(propertyName).GetValue(obj, null) as T;
    }
}
