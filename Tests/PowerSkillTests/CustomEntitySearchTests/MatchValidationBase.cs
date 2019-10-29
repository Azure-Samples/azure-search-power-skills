// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntityLookupTests
{
    public class MatchValidationBase
    {
        private const string TestJsonFileName = "testWords.json";
        private const string TestCsvFileName = "testWords.csv";

        private static readonly Func<HttpRequest, Task<IActionResult>> _entityLookupFunction =
                Helpers.CurrySkillFunction(CustomEntityLookup.RunCustomEntityLookup);

        public static CustomEntitiesDefinition GetEntitiesDefinition(
            int maximumFuziness = 0,
            bool caseSensitive = false,
            bool accentSensitive = false,
            params string[] words)
        {
            var targetCustomEntities = new List<CustomEntity>();

            foreach (var word in words)
            {
                var entity = new CustomEntity(
                    name: word,
                    description: $"description of {word}",
                    type: $"type of {word}",
                    subtype: $"subtype of {word}",
                    id: $"id of {word}",
                    caseSensitive: caseSensitive,
                    accentSensitive: accentSensitive,
                    fuzzyEditDistance: maximumFuziness,
                    defaultCaseSensitive: false,
                    defaultAccentSensitive: false,
                    defaultFuzzyEditDistance: 0,
                    aliases: null);
                targetCustomEntities.Add(entity);
            }

            var entitiesDefinition = new CustomEntitiesDefinition(targetCustomEntities);
            return entitiesDefinition;
        }

        public void TestFindMatch(
            string text,
            int allowableFuziness,
            int expectedMatches,
            double? expectedFuziness = null,
            bool caseSensitive = true,
            bool accentSensitive = true,
            bool findOnlyFirstMatch = false,
            params string[] words)
        {
            var entitiesDefinition = GetEntitiesDefinition(
                words: words, 
                caseSensitive: caseSensitive,
                accentSensitive: accentSensitive,
                maximumFuziness: allowableFuziness);

            ReplaceWordsJsonFile(JsonConvert.SerializeObject(entitiesDefinition));

            string input = BuildInput(text);
            var resultJson = QueryEntityLookupFunctionAndSerialize(input).GetAwaiter().GetResult();

            var result = JsonConvert.DeserializeObject<dynamic>(resultJson);
            var entities = ((JArray)result["values"][0]["data"]["entities"]).ToObject<List<FoundEntity>>();

            Assert.AreEqual(expectedMatches, entities.Sum(e => e.Matches.Count));

            if (expectedFuziness != null && expectedMatches > 0)
            {
                Assert.AreEqual(expectedFuziness, entities.First().Matches.First().MatchDistance);
            }
        }

        private static void ReplaceWordsJsonFile(string serializedConfig)
        {
            var executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var path = Path.Combine(executingLocation, TestJsonFileName);
            CustomEntityLookup.EntityDefinitionLocation = TestJsonFileName;

            File.WriteAllText(path, serializedConfig);
        }

        public static async Task<WebApiSkillResponse> QueryEntityLookupFunction(string inputText)
            => await Helpers.QueryFunction(inputText, _entityLookupFunction);

        private static async Task<string> QueryEntityLookupFunctionAndSerialize(string inputText)
            => await Helpers.QueryFunctionAndSerialize(inputText, _entityLookupFunction);

        public static string BuildInput(
            string text)
            => Helpers.BuildPayload(new
            {
                Text = text,
            });
    }
}
