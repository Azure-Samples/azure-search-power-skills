// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{
    public static class CustomEntitySearchHelpers
    {
        private static readonly Func<HttpRequest, Task<IActionResult>> _entitySearchFunction =
            Helpers.CurrySkillFunction(CustomEntitySearch.RunCustomEntitySearch);

        public static string BuildInput(string text, string[] words)
            => Helpers.BuildPayload(new
            {
                Text = text,
                Words = words
            });

        public static string BuildInput(string text, string[] words, Dictionary<string, string[]> synonyms, string[] exactMatches, int offset)
            => Helpers.BuildPayload(new
            {
                Text = text,
                Words = words,
                Synonyms = synonyms,
                ExactMatches = exactMatches,
                FuzzyMatchOffset = offset
            });

        public static string BuildOutput(string[] entities, string[] matches, int[] matchIndices)
            => Helpers.BuildPayload(new
            {
                Entities = matches.Select((entity, i) => new
                {
                    Category = "customEntity",
                    Value = entity,
                    Offset = matchIndices[i],
                    Confidence = 0.0
                }),
                EntitiesFound = entities
            });

        public static string BuildOutput(string[] entities, string[] matches, int[] matchIndices, double[] confidence)
            => Helpers.BuildPayload(new
            {
                Entities = matches.Select((entity, i) => new
                {
                    Category = "customEntity",
                    Value = entity,
                    Offset = matchIndices[i],
                    Confidence = confidence[i]
                }),
                EntitiesFound = entities
            });

        public static async Task CallEntitySearchFunctionAndCheckResults(
            string[] expectedFoundEntities, string[] expectedMatches, int[] expectedMatchIndices, double[] confidence,
            string text, string[] words, Dictionary<string, string[]> synonyms, string[] exactMatches, int offset,
            string warningMessage = "")
        {
            string input = BuildInput(text, words, synonyms, exactMatches, offset);
            string expectedOutput = BuildOutput(expectedFoundEntities, expectedMatches, expectedMatchIndices, confidence);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);
            if (warningMessage != "")
                expectedOutput = expectedOutput.Replace(@"""warnings"":[]", warningMessage);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        public static async Task CallEntitySearchFunctionAndCheckResults(
            string[] expectedFoundEntities, string[] expectedMatches, int[] expectedMatchIndices,
            string text, string[] words)
        {
            string input = BuildInput(text, words);
            string expectedOutput = BuildOutput(expectedFoundEntities, expectedMatches, expectedMatchIndices);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        public static async Task<WebApiSkillResponse> QueryEntitySearchFunction(string inputText)
            => await Helpers.QueryFunction(inputText, _entitySearchFunction);

        private static async Task<string> QueryEntitySearchFunctionAndSerialize(string inputText)
            => await Helpers.QueryFunctionAndSerialize(inputText, _entitySearchFunction);
    }
}