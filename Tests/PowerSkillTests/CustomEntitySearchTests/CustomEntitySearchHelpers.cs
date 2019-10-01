// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{
    public static class CustomEntitySearchHelpers
    {
        private static readonly Func<HttpRequest, Task<IActionResult>> _entitySearchFunction =
            Helpers.CurrySkillFunction(CustomEntitySearch.RunCustomEntitySearch);

        public static string BuildInput(
            string text)
            => Helpers.BuildPayload(new
            {
                Text = text,
            });

        public static string BuildOutput(
            string[] entities,
            string[] matches,
            int[] matchIndices)
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

        public static string BuildOutput(
            string[] entities,
            string[] matches,
            int[] matchIndices,
            double[] confidence)
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
            string[] expectedFoundEntities,
            string[] expectedMatches,
            int[] expectedMatchIndices,
            double[] confidence,
            string text,
            string[] words,
            Dictionary<string, string[]> synonyms,
            string[] exactMatches,
            int offset,
            string warningMessage = "",
            string errorMessage = "")
        {
            string input = BuildInput(text);
            ReplaceWordsJson(words, synonyms, exactMatches, offset);
            string expectedOutput = BuildOutput(expectedFoundEntities, expectedMatches, expectedMatchIndices, confidence);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);
            if (warningMessage != "")
                expectedOutput = expectedOutput.Replace(@"""warnings"":[]", warningMessage);
            if (errorMessage != "")
            {
                expectedOutput = expectedOutput.Replace(@"""errors"":[]", errorMessage);
                expectedOutput = expectedOutput.Remove(35, 32);
            }

            Helpers.AssertJsonEquals(expectedOutput, actualOutput);
        }

        public static async Task CallEntitySearchFunctionAndCheckResults(
            string text,
            string[] words,
            string expectedOutput,
            Dictionary<string, string[]> synonyms = null,
            string[] exactMatches = null,
            int offset = 0)
        {
            string input = BuildInput(text);
            ReplaceWordsJson(words, synonyms, exactMatches, offset);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);

            Helpers.AssertJsonEquals(expectedOutput, actualOutput);
        }

        public static void ReplaceWordsJson(
            string[] words,
            Dictionary<string, string[]> synonyms = null,
            string[] exactMatches = null,
            int offset = 0)
        {
            exactMatches = exactMatches ?? new string[0];
            synonyms = synonyms ?? new Dictionary<string, string[]>();

            JObject config = new JObject();
            config["words"] = JArray.FromObject(words);
            config["synonyms"] = JObject.FromObject(synonyms);
            config["exactMatch"] = JArray.FromObject(exactMatches);
            config["fuzzyEditDistance"] = offset;
            config["caseSensitive"] = true;

            var executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(executingLocation, "testWords.json");
            CustomEntitySearch.EntityDefinitionLocation = "testWords.json";

            var serializedConfig = config.ToString();
            File.WriteAllText(path, serializedConfig);
        }

        public static async Task CallEntitySearchFunctionAndCheckResults(
            string[] expectedFoundEntities,
            string[] expectedMatches,
            int[] expectedMatchIndices,
            string text,
            string[] words,
            string warningMessage = "",
            string errorMessage = "")
        {
            string input = BuildInput(text);
            ReplaceWordsJson(words);
            string expectedOutput = BuildOutput(expectedFoundEntities, expectedMatches, expectedMatchIndices);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);
            if (warningMessage != "")
                expectedOutput = expectedOutput.Replace(@"""warnings"":[]", warningMessage);
            if (errorMessage != "")
            {
                expectedOutput = expectedOutput.Replace(@"""errors"":[]", errorMessage);
                expectedOutput = expectedOutput.Remove(35, 32);
            }

            Helpers.AssertJsonEquals(expectedOutput, actualOutput);
        }

        private static void ReplaceWordsCsv(string[] words)
        {
            var executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(executingLocation, "testWords.csv");
            CustomEntitySearch.EntityDefinitionLocation = "testWords.csv";

            var serializedConfig = string.Join(Environment.NewLine, words);
            File.WriteAllText(path, serializedConfig);
        }

        public static async Task<WebApiSkillResponse> QueryEntitySearchFunction(string inputText)
            => await Helpers.QueryFunction(inputText, _entitySearchFunction);

        private static async Task<string> QueryEntitySearchFunctionAndSerialize(string inputText)
            => await Helpers.QueryFunctionAndSerialize(inputText, _entitySearchFunction);
    }
}