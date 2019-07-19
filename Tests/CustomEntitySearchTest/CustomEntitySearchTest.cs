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
            var jsonContent = new StringContent(inputText, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingWordsExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void MissingTextBadRequest()
        {
            // tests against incorrect input (missing text)
            string missingTextPayload = TestData.missingTextBadRequestInput;
            var jsonContent = new StringContent(missingTextPayload, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual("Bad Request", response.ReasonPhrase, false);
            Assert.AreEqual(TestData.missingTextExpectedResponse, responseString, false);
        }

        [TestMethod]
        public void EmptyTextWordsNotFound()
        {
            // tests against empty string text
            string emptyText = TestData.GetPayload(@"""""", TestData.emptyTextWordsNotFoundInput);
            var jsonContent = new StringContent(emptyText, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                 output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkEmptyTextWordsNotFound = TestData.GetOutput(TestData.emptyTextWordsNotFoundInput, @"-1");
            Assert.AreEqual(checkEmptyTextWordsNotFound, responseString, false);
        }

        [TestMethod]
        public void EmptyWordsEmptyEntities()
        {
            //tests against empty string words
            string emptyWords = TestData.GetPayload(TestData.emptyWordsEmptyEntitiesInput, @"""""");
            var jsonContent = new StringContent(emptyWords, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkEmptyWordsEmptyEntities = TestData.GetOutput(@"""""", @"-1");
            Assert.AreEqual(checkEmptyWordsEmptyEntities, responseString, false);
        }

        [TestMethod]
        public void LargeTextQuickResult()
        {
            // tests against large text string
            string largeText = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords);
            var jsonContent = new StringContent(largeText, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkLargeTextQuickResult = TestData.GetOutput(TestData.largeTextQuickResultInputWords, TestData.largeTextQuickResultExpectedOutput);
            Assert.AreEqual(checkLargeTextQuickResult, responseString, false);
        }

        [TestMethod]
        public void LargeWordsQuickResult()
        {
            // tests against large pattern in words array
            string largeWord = TestData.GetPayload(TestData.largeWordsQuickResultInputText, TestData.largeWordsQuickResultInputWords);
            var jsonContent = new StringContent(largeWord, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkLargeWordsQuickResult = TestData.GetOutput(TestData.largeWordsQuickResultInputWords, TestData.largeWordsQuickResultExpectedOutput);
            Assert.AreEqual(checkLargeWordsQuickResult, responseString, false);
        }

        [TestMethod]
        public void LargeDatasetQuickResult()
        {
            //tests against a large number of documents inputted (loadtest)
            string content = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords, TestData.numDocs);
            var jsonContent = new StringContent(content, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkLargeDatasetQuickResult = TestData.GetOutput(TestData.largeTextQuickResultInputWords, TestData.largeTextQuickResultExpectedOutput, TestData.numDocs);
            Assert.AreEqual(checkLargeDatasetQuickResult, responseString, false);
        }

        [TestMethod]
        public void LargeNumWordsQuickResult()
        {
            // tests against a large number of patterns in words array
            string largeNumWords = TestData.GetPayload(TestData.largestText, TestData.largestWords);
            var jsonContent = new StringContent(largeNumWords, null, "application/json");
            HttpResponseMessage response = client.PostAsync(TestData.hostAddress, jsonContent).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            var output = new WebApiResponseRecord();
            try
            {
                output = JsonConvert.DeserializeObject<WebApiResponseRecord>(responseString);
            }
            catch
            {
                Assert.Fail("Skill failed to handle an empty test. Errored out.");
            }
            string checkLargeNumWordsQuickResult = TestData.GetOutput(TestData.largestWords, TestData.largeNumWordsQuickResultExpectedOutput);
            Assert.AreEqual(checkLargeNumWordsQuickResult, responseString, false);
        } 
    }
}
