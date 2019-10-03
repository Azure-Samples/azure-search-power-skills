// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{

    [TestClass]
    public class CustomEntitySearchTests
    {
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
            //tests against a large input document
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
                TestData.LargestText,
                TestData.LargestWords,
                TestData.LargeNumWordsQuickResultExpectedResponse);
        }

        [TestMethod]
        public async Task SupportAllOtherCurrentLanguages()
        {
            TestData.supportedTextandWordsTempInitializer();
            Dictionary<string, string[]> supportedLangTextandWords = TestData.supportedTextandWords;
            Dictionary<string, int[]> matchIndices = TestData.supportedMatchIndices;
            foreach (string language in supportedLangTextandWords.Keys)
            {
                await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                new string[] { supportedLangTextandWords[language][3] }, supportedLangTextandWords[language][2].Split(", "),
                matchIndices[language], supportedLangTextandWords[language][0], new string[] { supportedLangTextandWords[language][1] });
            }

        }

        [TestMethod]
        public async Task NoDoubleCountedExactMatch()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.NoDoubleCountedExactMatchWords, TestData.NoDoubleCountedExactMatches,
                TestData.NoDoubleCountedExactMatchIndices, TestData.NoDoubleCountedExactMatchConfidence, TestData.NoDoubleCountedExactMatchText, 
                TestData.NoDoubleCountedExactMatchWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }

        [TestMethod]
        public async Task AccentsHalfMismatch()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.AccentsHalfMismatchWords,
                new string[] { TestData.AccentsHalfMismatchText }, new int[] { 0 }, new double[] { 0.5 }, TestData.AccentsHalfMismatchText,
                TestData.AccentsHalfMismatchWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }

        [TestMethod]
        public async Task OnlyFindEntitiesUnderOffsetLimit()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.OnlyFindEntitiesUnderOffsetLimitWords, TestData.OnlyFindEntitiesUnderOffsetLimitMatches,
                TestData.OnlyFindEntitiesUnderOffsetLimitIndices, TestData.OnlyFindEntitiesUnderOffsetLimitConfidence, TestData.OnlyFindEntitiesUnderOffsetLimitText,
                TestData.OnlyFindEntitiesUnderOffsetLimitWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }

        [TestMethod]
        public async Task FuzzyWordsLongerThanText()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.FuzzyWordsLongerThanTextWords, new[] { TestData.FuzzyWordsLongerThanTextText },
                TestData.FuzzyWordsLongerThanTextIndices, TestData.FuzzyWordsLongerThanTextConfidence, TestData.FuzzyWordsLongerThanTextText, 
                TestData.FuzzyWordsLongerThanTextWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }

        [TestMethod]
        public async Task FuzzyTextLongerThanWords()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.FuzzyTextLongerThanWordsWords, TestData.FuzzyTextLongerThanWordsMatches,
                TestData.FuzzyTextLongerThanWordsIndices, TestData.FuzzyTextLongerThanWordsConfidence, TestData.FuzzyTextLongerThanWordsText,
                TestData.FuzzyTextLongerThanWordsWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }

        [TestMethod]
        public async Task CheckFuzzySituationAllLang()
        {
            if (!TestData.supportedTextandWords.ContainsKey("Greek"))
                TestData.supportedTextandWordsTempInitializer();
            Dictionary<string, string[]> supportedLangTextandWords = TestData.supportedTextandWords;
            Dictionary<string, int[]> matchIndices = TestData.supportedMatchIndices;
            Dictionary<string, double[]> confidenceScore = TestData.supportedConfidence;
            foreach (string language in confidenceScore.Keys)
            {
                await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                new string[] { supportedLangTextandWords[language][4] }, supportedLangTextandWords[language][2].Split(", "),
                matchIndices[language], confidenceScore[language], supportedLangTextandWords[language][0],
                new string[] { supportedLangTextandWords[language][4] }, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
            }
        }

        [TestMethod]
        public async Task LargeLeniencyMismatchedWord()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.LargeLeniencyMismatchedWordWords, TestData.LargeLeniencyMismatchedWordMatches,
                TestData.LargeLeniencyMismatchedWordIndices, TestData.LargeLeniencyMismatchedWordConfidence, TestData.LargeLeniencyMismatchedWordText,
                TestData.LargeLeniencyMismatchedWordWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 2);
        }

        [TestMethod]
        public async Task WordSmallerThanLeniency()
        {
            CustomEntitySearchHelpers.ReplaceWordsJsonFile(words: new string[] { "i" }, offset: 2);

            WebApiSkillResponse outputContent = await CustomEntitySearchHelpers.QueryEntitySearchFunction(TestData.WordSmallerThanLeniencyInput);
            Assert.IsTrue(outputContent.Values[0].Warnings[0].Message.Contains(TestData.WordSmallerThanLeniencyWarning));
        }

        [TestMethod]
        public async Task LargeLeniencyMismatchedText()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.LargeLeniencyMismatchedTextWords, TestData.LargeLeniencyMismatchedTextMatches,
                TestData.LargeLeniencyMismatchedTextIndices, TestData.LargeLeniencyMismatchedTextConfidence, TestData.LargeLeniencyMismatchedTextText,
                TestData.LargeLeniencyMismatchedTextWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 3);
        }

        [TestMethod]
        public async Task LargeLeniencyMismatchedMix()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.LargeLeniencyMismatchedMixWords, TestData.LargeLeniencyMismatchedMixMatches,
                TestData.LargeLeniencyMismatchedMixIndices, TestData.LargeLeniencyMismatchedMixConfidence, TestData.LargeLeniencyMismatchedMixText,
                TestData.LargeLeniencyMismatchedMixWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 3);
        }

        [TestMethod]
        public async Task LargestLeniencyCheck()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(
                TestData.LargestLeniencyCheckText,
                TestData.LargestLeniencyCheckWords,
                TestData.LargestLeniencyCheckExpectedResponse,
                offset: 10);
        }

        [TestMethod]
        public async Task OverlapInText()
        {
            await CustomEntitySearchHelpers.CallEntitySearchFunctionAndCheckResults(TestData.OverlapInTextWords, TestData.OverlapInTextMatches,
                TestData.OverlapInTextIndices, TestData.OverlapInTextConfidence, TestData.OverlapInTextText,
                TestData.OverlapInTextWords, new Dictionary<string, string[]>(), Array.Empty<string>(), 1);
        }
    }
}
