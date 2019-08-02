using AzureCognitiveSearch.PowerSkills.Common;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntitySearchTests
{
    public static class CustomEntitySearchHelpers
    {
        private static readonly Func<HttpRequest, Task<IActionResult>> _entitySearchFunction =
            request => CustomEntitySearch.RunCustomEntitySearch(request, new LoggerFactory().CreateLogger("local"), new Microsoft.Azure.WebJobs.ExecutionContext());

        public static string BuildInput(string text, string[] words)
            => Helpers.BuildPayload(new
            {
                Text = text,
                Words = words
            });

        public static string BuildOutput(string[] entities, string[] matches, int[] matchIndices)
            => Helpers.BuildPayload(new
            {
                Entities = matches.Select((entity, i) => new
                {
                    Name = entity,
                    MatchIndex = matchIndices[i]
                }),
                EntitiesFound = entities
            });

        public static async Task<string> QueryEntitySearchFunctionAndSerialize(string inputText)
            => await Helpers.QueryFunctionAndSerialize(inputText, _entitySearchFunction);

        public static async Task<WebApiSkillResponse> QueryEntitySearchFunction(string inputText)
            => await Helpers.QueryFunction(inputText, _entitySearchFunction);

        public static async Task CallEntitySearchFunctionAndCheckResults(
            string[] expectedFoundEntities, string[] expectedMatches, int[] expectedMatchIndices,
            string text, string[] words)
        {
            string input = BuildInput(text, words);
            string expectedOutput = BuildOutput(expectedFoundEntities, expectedMatches, expectedMatchIndices);
            string actualOutput = await QueryEntitySearchFunctionAndSerialize(input);
            Assert.AreEqual(expectedOutput, actualOutput);
        }
    }
}
