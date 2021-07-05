// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Collections.Generic;
using System.Net.Http;
using AzureCognitiveSearch.PowerSkills.Text.TextAnalyticsForHealth;
using System;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureCognitiveSearch.PowerSkills.Tests.TextAnalyticsForHealthTests
{
    /** TODO: Need to come back and find a way to test the Function better. Currently the tests fail when using the
     * helper functions, which we believe is because of how the Text Analytics SDK sends requests to their endpoint.
     */
    [TestClass]
    public class TextAnalyticsForHealthTests
    {
        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            WebApiSkillHelpers.TestMode = true;
            WebApiSkillHelpers.TestWww = req =>
            {
                return req.RespondRequestWith(new
                {
                    jobId = "1",
                    status = "succeeded",
                    errors = new object[0],
                    results = new {
                        documents = new object[]
                        {
                            new
                            {
                               id = "1",
                               entities = new object[]
                               {
                                   new
                                   {
                                       offset = 25,
                                       length = 5,
                                       text = "100mg",
                                       category = "Dosage",
                                       confidenceScore = 1.0
                                   }
                               },
                               relations = new object[0]
                            }
                        }
                    },
                    response = "Hello"
                });
            };
        }

        [TestMethod]
        public async Task MissingSettings()
        {
            Environment.SetEnvironmentVariable(TextAnalyticsForHealth.textAnalyticsApiEndpointSetting, "https://testendpoint.com");
            var payload = new
            {
                document = "World"
            };
            var jsonContent = new StringContent(Helpers.BuildPayload(payload), null, "application/json");
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                ContentType = "application/json; charset=utf-8",
                Body = await jsonContent.ReadAsStreamAsync(),
                Method = "POST"
            };
            var response = await Helpers.CurrySkillFunction(TextAnalyticsForHealth.Run)(request) as ObjectResult;
            var expectedResponse = "unitTestFunction - TextAnalyticsForHealth API key or Endpoint is missing. Make sure to set it in the Environment Variables.";
            Assert.AreEqual(expectedResponse, response.Value);
        }
    }
}
