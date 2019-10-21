// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntityLookupTests
{
    [TestClass]
    [TestCategory("CustomEntityLookup")]
    public class MatchDistance1Fuzzy : MatchValidationBase
    {
        [TestMethod]
        public void TestSingleCharacterAdd()
        {
            TestFindMatch(
                text: "ab",
                words: "a",
                expectedMatches: 0);

            TestFindMatch(
                text: "ab",
                words: "b", 
                expectedMatches: 0);
        }

        [TestMethod]
        public void TestTwoCharacterReplace()
        {
            TestFindMatch(
                text: "ab",
                words: "az",
                expectedMatches: 1);

            TestFindMatch(
                text: "ab",
                words: "zb",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterTranspose()
        {
            TestFindMatch(
                text: "ab",
                words: "ba",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterAdd()
        {
            TestFindMatch(
                text: "abc",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "ac",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "bc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterRemove()
        {
            TestFindMatch(
                text: "a",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: "b",
                words: "ab",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterReplace()
        {
            TestFindMatch(
                text: "abc",
                words: "zbc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "azc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "abz",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterTranspose()
        {
            TestFindMatch(
                text: "abc",
                words: "bac",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "acb",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterAdd()
        {
            TestFindMatch(
                text: "zabc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azbc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abcz",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterRemove()
        {
            TestFindMatch(
                text: "ab",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "ac",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "bc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "abcz",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestSingleCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: " ab",
                words: "a",
                expectedMatches: 0);

            TestFindMatch(
                text: "ab ",
                words: "a", 
                expectedMatches: 0);

            TestFindMatch(
                text: " ab ",
                words: "a", 
                expectedMatches: 0);

            TestFindMatch(
                text: " ab",
                words: "b", 
                expectedMatches: 0);

            TestFindMatch(
                text: "ab ",
                words: "b",
                expectedMatches: 0);

            TestFindMatch(
                text: " ab ",
                words: "b",
                expectedMatches: 0);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesReplace()
        {
            TestFindMatch(
                text: "ab ",
                words: "az",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab",
                words: "az",
                expectedMatches: 1);

            TestFindMatch(
                text: "ab ",
                words: "zb",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab",
                words: "zb",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesTranspose()
        {
            TestFindMatch(
                text: "ab ",
                words: "ba",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab",
                words: "ba",
                expectedMatches: 1);

            TestFindMatch(
                text: "ab ",
                words: "ba",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab",
                words: "ba",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: "abc ",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "ac",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "ac",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "ac",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "bc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "bc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "bc",
                expectedMatches: 1);

            TestFindMatch(
                text: "a b",
                words: "ab",
                expectedMatches: 2);

            TestFindMatch(
                text: " a b",
                words: "ab",
                expectedMatches: 2);

            TestFindMatch(
                text: "a b ",
                words: "ab",
                expectedMatches: 2);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesRemove()
        {
            TestFindMatch(
                text: "a ",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " a",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " a ",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " b",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: "b ",
                words: "ab",
                expectedMatches: 1);

            TestFindMatch(
                text: " b ",
                words: "ab",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesReplace()
        {
            TestFindMatch(
                text: " abc",
                words: "zbc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "zbc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "zbc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "azc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "azc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "azc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "abz",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "abz",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "abz",
                expectedMatches: 1);
 
            TestFindMatch(
                text: "z bc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "z ab ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesTranspose()
        {
            TestFindMatch(
                text: " abc",
                words: "bac",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "acb",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "bac",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "acb",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "bac",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "acb",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "bac",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "acb",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "bac",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: " zabc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "zabc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " zabc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " azbc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azbc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " azbc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abzc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abzc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcz",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcz ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesRemove()
        {
            TestFindMatch(
                text: " bc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "bc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " bc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "ab ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " ab ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " ac",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "ac ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " ac ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestMultipleMatches()
        {
            TestFindMatch(
                text: "ad cb ab be ad ac bd",
                words: "ab",
                expectedMatches: 5);
        }

        [TestMethod]
        public void TestMultipleMultiCharacterMatch()
        {
            TestFindMatch(
                text: "acbz acbd ac bd acb abd cbd c d e a d ac c bd d",
                words: "acbd",
                expectedMatches: 5);
        }

        [TestMethod]
        public void CaseInsensitivityDoesntCountAgainstFuzinessWithWords()
        {
            TestFindMatch(
                text: "   abcd  Abcd  aBcd  xyz abCd abcD  AB  AC  BC  ABCD BAC bac ACB aCb acB aZc ABZ ",
                words: new string[] { "abc" },
                caseSensitive: false,
                expectedMatches: 16);
        }

        [TestMethod]
        public void CaseInsensitivityDoesntCountAgainstFuzinessWithText()
        {
            TestFindMatch(
                text: "   abcd bac ab azc defg dfz edf ef ghij zhi hgi gh gzi jklm jlk jl jzl mnor nmo mn zno pqrs pqr pr qpr stvw stv szv tsv sv wxyz wxy wzy wy z 123 ",
                words: new string[] { "Abc", "dEf", "ghI", "JKl", "MnO", "pQR", "STV", "wxy" },
                caseSensitive: false,
                expectedMatches: 33);
        }

        [TestMethod]
        public void AccentInsensitivityDoesntCountAgainstFuzinessWithWords()
        {
            TestFindMatch(
                text: "   àbcz zàbc abcz axbc aàc aà àc aac ac aa abà baà baà aba baa aab àzc àbz zbc ",
                words: new string[] { "abc", "aac", "aba" },
                accentSensitive: false,
                expectedMatches: 30);
        }

        [TestMethod]
        public void AccentInsensitivityDoesntCountAgainstFuzinessWithText()
        {
            TestFindMatch(
                text: "   àbcd abc aàc aac abà aba  àbz àzc àbc bàc àcb àbc zàbc àc àb",
                words: new string[] { "àbc" },
                accentSensitive: false,
                expectedMatches: 15);
        }


        [TestMethod]
        public void AccentAndCaseInsensitivityDoesntCountAgainstFuziness()
        {
            TestFindMatch(
                text: "   ÀBCD ÀDbC DÀbc àB àC àcb ÀAC AàC Aàc aÀC aàZ zàc BAÀ BAà baÀ baà xyz 123",
                words: new string[] { "abc", "aac", "aba" },
                caseSensitive: false,
                accentSensitive: false,
                expectedMatches: 22);
        }

        public void TestFindMatch(
            string text,
            int expectedMatches,
            bool caseSensitive = false,
            bool accentSensitive = false,
            params string[] words)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 1,
                expectedMatches: expectedMatches,
                expectedFuziness: 1,
                caseSensitive: caseSensitive,
                accentSensitive: accentSensitive);
        }
    }
}
