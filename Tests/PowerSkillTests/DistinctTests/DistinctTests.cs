// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.VisualStudio.TestTools.UnitTesting;
using AzureCognitiveSearch.PowerSkills.Text.Distinct;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace AzureCognitiveSearch.PowerSkills.Tests.DistinctTests
{
    [TestClass]
    public class DistinctTests
    {
        [DataTestMethod]
        [DataRow("Case insensitive díàcrïtiçs señsîtive AaBbCcÉéeÀàaÇçcÑñn", "caseinsensitivedíàcrïtiçsseñsîtiveaabbccééeààaççcññn")]
        [DataRow("Spaces are removed", "spacesareremoved")]
        [DataRow("Punctuation, as well; that is good...", "punctuationaswellthatisgood")]
        public void NormalizeLowersCaseRemovesPunctuationAndSpaces(string word, string expectedNormalized)
            => Assert.AreEqual(expectedNormalized.Normalize(), Thesaurus.Normalize(word));

        private static readonly string[][] _synonyms
            = new[]
            {
                new[] { "A.C.R.O.N.Y.M", "acornym", "acronyms" },
                new[] { "Microsoft", "Microsoft Corporation", "Microsoft corp.", "MSFT" }
            };

        [DataTestMethod]
        [DataRow(new[] { "Acronym", "acornym", "a cro:n y.m "}, "A.C.R.O.N.Y.M")]
        [DataRow(new[] { "miCrosoft", "microsoft Corp;", "M.S.F.T."}, "Microsoft")]
        [DataRow(new[] { "Not found" }, "Not found")]
        public void ThesaurusReturnsCanonicalFormOrWordIfNotFound(IEnumerable<string> words, string expectedCanonical)
        {
            foreach (string word in words)
            {
                Assert.AreEqual(expectedCanonical, new Thesaurus(_synonyms).Dedupe(new[] { word }).First());
            }
        }

        [DataTestMethod]
        [DataRow(
            "It is true that many acronyms are used at Microsoft. MSFT is no different as it's just an acronym for Microsoft.",
            "It is true that many A.C.R.O.N.Y.M are used at Microsoft no different as it's just an for")]
        public void DeduplicateYieldsListOfDistinctCanonicalForms(string text, string expectedDeduplicated)
        {
            var expectedTerms = expectedDeduplicated.Split(' ').OrderBy(term => term);
            var deduped = new Thesaurus(_synonyms).Dedupe(text.Split(' ')).OrderBy(term => term);

            Assert.IsTrue(expectedTerms.SequenceEqual(deduped), $"Expected [{ string.Join(", ", expectedTerms) }] but was [{ string.Join(", ", deduped) }].");
        }

        [TestMethod]
        public void ThesaurusBuildsNormalizedSynonymToCanonicalFormDictionaryAndIgnoresEmptyLemmas()
        {
            const string canonicalAcronym = "acronym";
            const string canonicalMicrosoft = "Microsoft";
            var synonyms = new Thesaurus(new[]
            {
                new[] { canonicalAcronym, "acornym", "acronyms" },
                Array.Empty<string>(),
                new[] { canonicalMicrosoft, "Microsoft Corporation", "Microsoft corp.", "MSFT" }
            }).Synonyms;

            Assert.AreEqual(7, synonyms.Count());
            Assert.AreEqual(canonicalAcronym, synonyms["acronym"]);
            Assert.AreEqual(canonicalAcronym, synonyms["acornym"]);
            Assert.AreEqual(canonicalAcronym, synonyms["acronyms"]);
            Assert.AreEqual(canonicalMicrosoft, synonyms["microsoft"]);
            Assert.AreEqual(canonicalMicrosoft, synonyms["microsoftcorporation"]);
            Assert.AreEqual(canonicalMicrosoft, synonyms["microsoftcorp"]);
            Assert.AreEqual(canonicalMicrosoft, synonyms["msft"]);
        }

        [TestMethod]
        public void ThesaurusConstructorThrowsForDuplicateLemmas()
        {
            Assert.ThrowsException<InvalidDataException>(() => {
                _ = new Thesaurus(new[]
                {
                    new[] {"foo", "bar"},
                    new[] {"baz", "bar"}
                });
            });
        }
    }
}
