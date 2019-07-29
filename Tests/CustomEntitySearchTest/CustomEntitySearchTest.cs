using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Net.Http;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTest
{

    [TestClass]
    public class CustomEntitySearchTest
    {        
        [TestMethod]
        public async Task MissingWordsBadRequest()
        {
            // tests against incorrect input (missing words)
            var outputContent = await TestData.GeneratePayloadRequest(TestData.missingWordsBadRequestInput);
            Assert.AreEqual(TestData.missingWordsExpectedResponse, outputContent.Values[0].Errors[0].Message, false);
        }

        [TestMethod]
        public async Task MissingTextBadRequest()
        {
            // tests against incorrect input (missing text)
            var outputContent = await TestData.GeneratePayloadRequest(TestData.missingTextBadRequestInput);
            Assert.AreEqual(TestData.missingTextExpectedResponse, outputContent.Values[0].Errors[0].Message, false);
        }

        [TestMethod]
        public async Task EmptyTextWordsNotFound()
        {
            // tests against empty string text
            string emptyText = TestData.GetPayload(@"""""", TestData.emptyTextWordsNotFoundInput);
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(emptyText));
            var checkEmptyTextWordsNotFound = TestData.GetOutput("", "", "");
            Assert.AreEqual(checkEmptyTextWordsNotFound, outputContent, false);
        }

        [TestMethod]
        public async Task EmptyWordsEmptyEntities()
        {
            //tests against empty string words
            string emptyWords = TestData.GetPayload(TestData.emptyWordsEmptyEntitiesInput, @"""""");
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(emptyWords));
            string checkEmptyWordsEmptyEntities = TestData.GetOutput("", "","");
            Assert.AreEqual(checkEmptyWordsEmptyEntities, outputContent, false);
        }

        [TestMethod]
        public async Task LargeTextQuickResult()
        {
            // tests against large text string
            string largeText = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords);
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(largeText));
            string checkLargeTextQuickResult = TestData.GetOutput(TestData.largeTextOutputNames, TestData.largeTextOutputMatchIndex, TestData.largeTextOutputFound);
            Assert.AreEqual(checkLargeTextQuickResult, outputContent, false);
        }

        [TestMethod]
        public async Task LargeWordsQuickResult()
        {
            // tests against large pattern in words array
            string largeWord = TestData.GetPayload(TestData.largeWordsQuickResultInputText, TestData.largeWordsQuickResultInputWords);
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(largeWord));
            string checkLargeWordsQuickResult = TestData.GetOutput(TestData.largeWordsOutputNames, TestData.largeWordsOutputMatchIndex, TestData.largeWordsOutputFound);
            Assert.AreEqual(checkLargeWordsQuickResult, outputContent, false);
        }

        [TestMethod]
        public async Task LargeDatasetQuickResult()
        {
            //tests against a large number of documents inputted (loadtest)
            string largeDataset = TestData.GetPayload(TestData.largestText, TestData.largeTextQuickResultInputWords, TestData.numDocs);
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(largeDataset));
            string checkLargeDatasetQuickResult = TestData.GetOutput(TestData.largeTextOutputNames, TestData.largeTextOutputMatchIndex, TestData.largeTextOutputFound, TestData.numDocs);
            Assert.AreEqual(checkLargeDatasetQuickResult, outputContent, false);
        }

        [TestMethod]
        public async Task LargeNumWordsQuickResult()
        {
            // tests against a large number of patterns in words array
            string largeNumWords = TestData.GetPayload(TestData.largestText, TestData.largestWords);
            var outputContent = JsonConvert.SerializeObject(await TestData.GeneratePayloadRequest(largeNumWords));
            string checkLargeNumWordsQuickResult = TestData.GetOutput(TestData.largeNumWordsOutputNames, TestData.largeNumWordsOutputMatchIndex, TestData.largeNumWordsOutputFound);
            Assert.AreEqual(checkLargeNumWordsQuickResult, outputContent, false);
        }
    }
}
