using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{

    [TestClass]
    public class CustomEntitySearchTests
    {        
        [TestMethod]
        public async Task MissingWordsBadRequest()
        {
            // tests against incorrect input (missing words)
            WebApiSkillResponse outputContent = await CustomEntitySearchHelpers.QueryEntitySearchFunction(TestData.MissingWordsBadRequestInput);
            Assert.IsTrue(outputContent.Values[0].Errors[0].Message.Contains(TestData.MissingWordsExpectedResponse));
        }

        [TestMethod]
        public async Task MissingTextBadRequest()
        {
            // tests against incorrect input (missing text)
            WebApiSkillResponse outputContent = await CustomEntitySearchHelpers.QueryEntitySearchFunction(TestData.MissingTextBadRequestInput);
            Assert.IsTrue(outputContent.Values[0].Errors[0].Message.Contains(TestData.MissingTextExpectedResponse));
        }

        [TestMethod]
        public async Task EmptyTextWordsNotFound()
        {
            // tests against empty string text
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<int>(),
                "", TestData.EmptyTextWordsNotFoundInput);
        }

        [TestMethod]
        public async Task EmptyWordsEmptyEntities()
        {
            //tests against empty string words
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<int>(),
                "", Array.Empty<string>());
        }

        [TestMethod]
        public async Task LargeTextQuickResult()
        {
            // tests against large text string
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                TestData.LargeTextOutputFound, TestData.LargeTextOutputNames, TestData.LargeTextOutputMatchIndex,
                TestData.LargestText, TestData.LargeTextQuickResultInputWords);
        }

        [TestMethod]
        public async Task LargeWordsQuickResult()
        {
            // tests against large pattern in words array
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                TestData.LargeWordsOutputFound, TestData.LargeWordsOutputNames, TestData.LargeWordsOutputMatchIndex,
                TestData.LargeWordsQuickResultInputText, TestData.LargeWordsQuickResultInputWords);
        }

        [TestMethod]
        public async Task LargeDatasetQuickResult()
        {
            //tests against a large number of documents inputted (loadtest)
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                TestData.LargeTextOutputFound, TestData.LargeTextOutputNames, TestData.LargeTextOutputMatchIndex,
                TestData.LargestText, TestData.LargeTextQuickResultInputWords);
            // TestData.NumDocs 2300
        }

        [TestMethod]
        public async Task LargeNumWordsQuickResult()
        {
            // tests against a large number of patterns in words array
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                TestData.LargeNumWordsOutputFound, TestData.LargeNumWordsOutputNames, TestData.LargeNumWordsOutputMatchIndex,
                TestData.LargestText, TestData.LargestWords);
        }
    }
}
