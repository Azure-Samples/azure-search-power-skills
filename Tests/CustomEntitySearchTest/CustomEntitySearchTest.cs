using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using AzureCognitiveSearch.PowerSkills.Common;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTest
{

    [TestClass]
    public class CustomEntitySearchTest
    {
        private static readonly HttpClient client = new HttpClient();
        
        [TestMethod]
        public void MissingWordsBadRequest()
        {
            // tests against incorrect input (missing words)
            string inputText = TestData.missingWordsBadRequestInput;
            HttpContent jsonContent = new StringContent(inputText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingWordsExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void MissingTextBadRequest()
        {
            // tests against incorrect input (missing text)
            string missingTextPayload = TestData.missingTextBadRequestInput;
            HttpContent jsonContent = new StringContent(missingTextPayload, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingTextExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void EmptyTextWordsNotFound()
        {
            // tests against empty string text
            string emptyText = TestData.GetPayload(@"""""", TestData.emptyTextWordsNotFoundInput);
            HttpContent jsonContent = new StringContent(emptyText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                 output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest3 = TestData.GetOutput(TestData.emptyTextWordsNotFoundInput, @"-1");
            Assert.AreEqual(checkTest3, responseString, false);
        }

        [TestMethod]
        public void EmptyWordsEmptyEntities()
        {
            //tests against empty string words
            string emptyWords = TestData.GetPayload(TestData.emptyWordsEmptyEntitiesInput, @"""""");
            HttpContent jsonContent = new StringContent(emptyWords, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest4 = TestData.GetOutput(@"""""", @"-1");
            Assert.AreEqual(checkTest4, responseString, false);
        }

        [TestMethod]
        public void LargeTextQuickResult()
        {
            // tests against large text string
            string largeText = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords);
            HttpContent jsonContent = new StringContent(largeText, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest5 = TestData.GetOutput(TestData.largeTextQuickResultInputWords, TestData.largeTextQuickResultExpectedOutput);
            Assert.AreEqual(checkTest5, responseString, false);
        }

        [TestMethod]
        public void LargeWordsQuickResult()
        {
            // tests against large pattern in words array
            string largeWord = TestData.GetPayload(TestData.largeWordsQuickResultInputText, TestData.largeWordsQuickResultInputWords);
            HttpContent jsonContent = new StringContent(largeWord, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest6 = TestData.GetOutput(TestData.largeWordsQuickResultInputWords, TestData.largeWordsQuickResultExpectedOutput);
            Assert.AreEqual(checkTest6, responseString, false);
        }

        [TestMethod]
        public void LargeDatasetQuickResult()
        {
            //tests against a large number of documents inputted (loadtest)
            string content = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords, TestData.numDocs);
            HttpContent jsonContent = new StringContent(content, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest7 = TestData.GetOutput(TestData.largeTextQuickResultInputWords, TestData.largeTextQuickResultExpectedOutput, TestData.numDocs);
            Assert.AreEqual(checkTest7, responseString, false);
        }

        [TestMethod]
        public void LargeNumWordsQuickResult()
        {
            // tests against a large number of patterns in words array
            string largeNumWords = TestData.GetPayload(TestData.largestText, TestData.largestWords);
            HttpContent jsonContent = new StringContent(largeNumWords, null, "application/json");
            var response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            WebApiResponseRecord output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkTest8 = TestData.GetOutput(TestData.largestWords, TestData.largeNumWordsQuickResultExpectedOutput);
            Assert.AreEqual(checkTest8, responseString, false);
        } 
    }
}
