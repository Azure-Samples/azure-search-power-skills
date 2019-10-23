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
    public class MatchDistance2Fuzzy : MatchValidationBase
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
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "ab",
                words: "zb",
                expectedMatches: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void TestTwoCharacterAdd()
        {
            TestFindMatch(
                text: "abcd",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "abcd",
                words: "acd",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "abcd",
                words: "bcd",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "abcde",
                words: "abc", 
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde",
                words: "acd", 
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde",
                words: "bcd", 
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde",
                words: "bde",
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde",
                words: "cde",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestTwoCharacterRemove()
        {
            TestFindMatch(
                text: "a",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "b",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void TestThreeCharacterReplace()
        {
            TestFindMatch(
                text: "abc",
                words: "zzc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "zbz",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc",
                words: "azz",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterAdd()
        {
            TestFindMatch(
                text: "zzabc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azzbc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abzzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abczz",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "zazbc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "zabzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "zabcz",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azbzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azbcz",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abzcz",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterRemove()
        {
            TestFindMatch(
                text: "ab",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "ac",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "bc",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "abc",
                words: "abcz",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "a",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "b",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "c",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestSingleCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: " abc",
                words: "a",
                expectedMatches: 0);

            TestFindMatch(
                text: "abc ",
                words: "a",
                expectedMatches: 0);

            TestFindMatch(
                text: " abc ",
                words: "a",
                expectedMatches: 0);

            TestFindMatch(
                text: " abc",
                words: "b", 
                expectedMatches: 0);

            TestFindMatch(
                text: "abc ",
                words: "b",
                expectedMatches: 0);

            TestFindMatch(
                text: " abc ",
                words: "b",
                expectedMatches: 0);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesReplace()
        {
            TestFindMatch(
                text: "ab ",
                words: "az",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ab",
                words: "az",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "ab ",
                words: "zb",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ab",
                words: "zb",
                expectedMatches: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: "abcde ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde ",
                words: "acd",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde",
                words: "acd",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde ",
                words: "acd",
                expectedMatches: 1);

            TestFindMatch(
                text: "abcde ",
                words: "bcd",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde",
                words: "bcd",
                expectedMatches: 1);

            TestFindMatch(
                text: " abcde ",
                words: "bcd",
                expectedMatches: 1);

            TestFindMatch(
                text: "az b",
                words: "abc",
                expectedMatches: 2);

            TestFindMatch(
                text: " a zb",
                words: "abc",
                expectedMatches: 2);

            TestFindMatch(
                text: " a zbd",
                words: "abc",
                expectedMatches: 2);

            TestFindMatch(
                text: "a zb ",
                words: "abc",
                expectedMatches: 2,
                expectedFuziness: 2);
        }

        [TestMethod]
        public void TestTwoCharacterWithSpacesRemove()
        {
            TestFindMatch(
                text: "a ",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " a",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " a ",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " b",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "b ",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " b ",
                words: "ab",
                expectedMatches: 1,
                expectedFuziness: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesReplace()
        {
            TestFindMatch(
                text: " abc",
                words: "zzc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "zzc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "zzc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "azz",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "azz",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "azz",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc",
                words: "zbz",
                expectedMatches: 1);

            TestFindMatch(
                text: "abc ",
                words: "zbz",
                expectedMatches: 1);

            TestFindMatch(
                text: " abc ",
                words: "zbz",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesAdd()
        {
            TestFindMatch(
                text: " zazbc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "zabzc ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " zabcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " azbzc",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "azbcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " azbcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abzcz",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "abzz c ",
                words: "abc",
                expectedMatches: 2);

            TestFindMatch(
                text: " abzcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " a bcz",
                words: "abc",
                expectedMatches: 2);

            TestFindMatch(
                text: "azbcz ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " abzcz ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestThreeCharacterWithSpacesRemove()
        {
            TestFindMatch(
                text: " bc",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "bc ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " bc ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ab",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "ab ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ab ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ac",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "ac ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: " ac ",
                words: "abc",
                expectedMatches: 1,
                expectedFuziness: 1);

            TestFindMatch(
                text: "   a   ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: "   b ",
                words: "abc",
                expectedMatches: 1);

            TestFindMatch(
                text: " c   ",
                words: "abc",
                expectedMatches: 1);
        }

        [TestMethod]
        public void TestMultipleMatchesWithOverlapedMatches()
        {
            TestFindMatch(
                text: "ad cb ab be ad ac bd",
                words: new string[] { "abc", "bcd" },
                expectedMatches: 13);

            TestFindMatch(
                text: "ad cb ab be ad ac bd",
                words: new string[] { "az", "bc" },
                expectedMatches: 8);
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
                expectedMatches: 34);
        }

        [TestMethod]
        public void AccentInsensitivityDoesntCountAgainstFuzinessWithWords()
        {
            TestFindMatch(
                text: "   àbcz zàbc abcz axbc aàc aà àc aac ac aa abà baà baà aba baa aab àzc àbz zbc ",
                words: new string[] { "abc", "aac", "aba" },
                accentSensitive: false,
                expectedMatches: 57);
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
                expectedMatches: 47);
        }

        public void TestFindMatch(
            string text,
            int expectedMatches,
            int? expectedFuziness = null,
            bool caseSensitive = true,
            bool accentSensitive = true,
            params string[] words)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 2,
                expectedMatches: expectedMatches,
                expectedFuziness: expectedFuziness,
                caseSensitive: caseSensitive,
                accentSensitive: accentSensitive);
        }
    }
}
