// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.AcronymLinker
{
    [TestClass]
    public class AcronymLinkerTests
    {
        private static readonly Dictionary<string, string> _acronyms = new Dictionary<string, string>
        {
            { "ML", "Machine Learning" },
            { "AI", "Artificial Intelligence" }
        };

        [ClassInitialize]
        public static void Setup(TestContext context)
            => Text.AcronymLinker.AcronymLinker.TestDataSet = _acronyms;

        [TestMethod]
        public async Task UnknownAcronymYieldsNull()
        {
            Assert.IsNull((await GetAcronym("Unknown")).Value);
        }

        [TestMethod]
        public async Task KnownAcronymIsFound()
        {
            foreach (string acronym in _acronyms.Keys)
            {
                var (Value, Description) = await GetAcronym(acronym);
                Assert.AreEqual(acronym, Value);
                Assert.AreEqual(_acronyms[acronym], Description);
            }
        }

        [TestMethod]
        public async Task AcronymIsOnlyFoundInAllCaps()
        {
            Assert.IsNull((await GetAcronym("ml")).Value);
            Assert.IsNull((await GetAcronym("Ml")).Value);
            Assert.IsNull((await GetAcronym("mL")).Value);
            Assert.IsNotNull((await GetAcronym("ML")).Value);
        }

        [TestMethod]
        public async Task UnknownAcronymYieldsEmptyList()
        {
            Dictionary<string, string> acronymsResponse = await GetAcronyms("This", "has", "no", "acronyms");
            Assert.AreEqual(0, acronymsResponse.Count);
        }

        [TestMethod]
        public async Task KnownAcronymsAreFound()
        {
            Dictionary<string, string> acronyms = await GetAcronyms("foo", "ML", "bar", "AI", "baz", "ML", "bah");
            Assert.AreEqual(2, acronyms.Count);
            string ml = acronyms["ML"];
            Assert.AreEqual(_acronyms["ML"], ml);
            string ai = acronyms["AI"];
            Assert.AreEqual(_acronyms["AI"], ai);
        }

        private static async Task<(string Value, string Description)> GetAcronym(string word)
        {
            object acronym = await Helpers.QuerySkill(
                Text.AcronymLinker.LinkAcronyms.RunAcronymLinker,
                new { Word = word },
                "acronym");
            if (acronym is null) return (null, null);
            return (acronym.GetProperty<string>("value"), acronym.GetProperty<string>("description"));
        }

        private static async Task<Dictionary<string, string>> GetAcronyms(params string[] words)
        {
            return ((object[]) await Helpers.QuerySkill(
                Text.AcronymLinker.LinkAcronyms.RunAcronymLinkerForLists,
                new { Words = words },
                "acronyms"))
                .ToDictionary(
                    acronym => acronym.GetProperty<string>("value"),
                    acronym => acronym.GetProperty<string>("description"));
        }
    }
}
