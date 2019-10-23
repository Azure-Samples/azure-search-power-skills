// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntityLookupTests
{
    [TestClass]
    [TestCategory("CustomEntityLookup")]
    public class MatchDistance0Fuzzy : MatchValidationBase
    {
        [TestMethod]
        public void TestSingleLetterMatch()
        {
            TestFindMatch(
                text: "a",
                words: "a",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatchWithSpaces()
        {
            TestFindMatch(
                text: "  abc  ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatchWithTrailingSpaces()
        {
            TestFindMatch(
                text: "abc   ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatchWithWordPrefixSpaces()
        {
            TestFindMatch(
                text: "abc",
                words: "   abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatchWithWordTrailingSpaces()
        {
            TestFindMatch(
                text: "abc",
                words: "abc   ",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestExactMatchWithWordSpaces()
        {
            TestFindMatch(
                text: "abc",
                words: "  abc   ",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestMultipleExactMatch()
        {
            TestFindMatch(
                text: "a  b a c a z d rrf",
                words: "a",
                expectedMatches: 3);
        }

        [TestMethod]
        public void TestMultipleExactMatchWithMultiplewords()
        {
            TestFindMatch(
                text: "a b c d a b d e a d a c b d",
                words: new string[] { "a", "d"},
                expectedMatches: 8);
        }

        [TestMethod]
        public void TestMultipleExactMatchWithWordPrefixSpaces()
        {
            TestFindMatch(
                text: "a  b a c a",
                words: "   a",
                expectedMatches: 3);
        }

        [TestMethod]
        public void TestMultipleExactMatchWithSpacesWithWordTrailingSpaces()
        {
            TestFindMatch(
                text: "a b c d a b d e a d a c b d",
                words: "a   ",
                expectedMatches: 4);
        }

        [TestMethod]
        public void TestMultipleExactMultiCharacterMatchWithSpaces()
        {
            TestFindMatch(
                text: "ac bd ac d ac bd ac b a bd c bd c d e a d ac c bd d",
                words: new string[] { "ac", "bd" },
                expectedMatches: 10);
        }

        [TestMethod]
        public void TestCaseInsensitiveMatchesWithWords()
        {
            TestFindMatch(
                text: "   abc  Abc  aBc  xyz abC  ABc  AbC  cba  aBC  ABC  ",
                words: new string[] { "abc"},
                caseSensitive: false,
                expectedMatches: 8);
        }

        [TestMethod]
        public void TestCaseInsensitiveMatchesWithText()
        {
            TestFindMatch(
                text: "   abc def ghi jkl mno pqr stv wxy z 123 ",
                words: new string[] { "Abc", "dEf", "ghI", "JKl", "MnO", "pQR", "STV", "wxy" },
                caseSensitive: false,
                expectedMatches: 8);
        }

        [TestMethod]
        public void TestAccentSensitiveDoesntWorkWithExactMatchWords()
        {
            TestFindMatch(
                text: "   àbc abc aàc aac abà aba ",
                words: new string[] { "abc", "aac", "aba" },
                accentSensitive: false,
                expectedMatches: 3);
        }

        [TestMethod]
        public void TestAccentSensitiveDoesntWorkWithExactMatch()
        {
            TestFindMatch(
                text: "   àbc abc aàc aac abà aba ",
                words: new string[] { "àbc", "aàc", "abà" },
                accentSensitive: false,
                expectedMatches: 3);
        }

        public void TestFindMatch(
            string text,
            int expectedMatches, 
            bool caseSensitive = true,
            bool accentSensitive = true,
            params string[] words)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 0,
                expectedMatches: expectedMatches,
                expectedFuziness: 0,
                caseSensitive: caseSensitive,
                accentSensitive: accentSensitive);
        }
    }
}
