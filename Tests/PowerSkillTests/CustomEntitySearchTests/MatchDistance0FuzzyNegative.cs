// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureCognitiveSearch.PowerSkills.Tests.CustomEntityLookupTests
{
    [TestClass]
    [TestCategory("CustomEntityLookup")]
    public class MatchDistance0FuzzyNegative : MatchValidationBase
    {
        [TestMethod]
        public void TestEmptyMatch()
        {
            TestFindMatch(
                text: "",
                words: "a");
        }

        [TestMethod]
        public void TestSingleLetterMisMatch()
        {
            TestFindMatch(
                text: "a",
                words: "z");
        }

        [TestMethod]
        public void TestSingleLetterCaseMisMatch()
        {
            TestFindMatch(
                text: "a",
                words: "A");
        }

        [TestMethod]
        public void TestSingleLetterDiacriticMisMatch()
        {
            TestFindMatch(
                text: "o",
                words: "ό");
        }

        [TestMethod]
        public void TestFirstLetterMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "zbc");
        }

        [TestMethod]
        public void TestMiddleLetterMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "azc");
        }

        [TestMethod]
        public void TestEndLetterMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "abz");
        }

        [TestMethod]
        public void TestNoTranspose()
        {
            TestFindMatch(
                text: "abc",
                words: "bac");

            TestFindMatch(
                text: "abc",
                words: "acb");
        }

        [TestMethod]
        public void TestFirstCharacterCaseMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "Abc");
        }

        [TestMethod]
        public void TestMiddleCharacterCaseMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "aBc");
        }

        [TestMethod]
        public void TestEndCharacterCaseMisMatch()
        {
            TestFindMatch(
                text: "abc",
                words: "abC");
        }

        [TestMethod]
        public void TestFirstCharacterDiacriticMisMatch()
        {
            TestFindMatch(
                text: "obc",
                words: "όbc");
        }

        [TestMethod]
        public void TestMiddleCharacterDiacriticMisMatch()
        {
            TestFindMatch(
                text: "aoc",
                words: "aόc");
        }

        [TestMethod]
        public void TestEndCharacterDiacriticMisMatch()
        {
            TestFindMatch(
                text: "abo",
                words: "abό");
        }

        [TestMethod]
        public void TestFirstLetterMismatchWithSpacePrefixes()
        {
            TestFindMatch(
                text: "   abc",
                words: "zbc");
        }


        [TestMethod]
        public void TestFirstLetterMismatchWithSpacePrefixesOnWord()
        {
            TestFindMatch(
                text: "abc",
                words: "    zbc");
        }

        [TestMethod]
        public void TestFirstLetterMismatchWithSpacePrefixesOnBoth()
        {
            TestFindMatch(
                text: " abc",
                words: " zbc");
        }

        [TestMethod]
        public void TestFirstLetterMismatchWithTrailingSpaces()
        {
            TestFindMatch(
                text: "abc   ",
                words: "zbc");
        }


        [TestMethod]
        public void TestFirstLetterMismatchWithTrailingSpacesOnWord()
        {
            TestFindMatch(
                text: "abc",
                words: "zbc    ");
        }

        [TestMethod]
        public void TestFirstLetterMismatchWithTrailingSpacesOnBoth()
        {
            TestFindMatch(
                text: "abc ",
                words: "zbc ");
        }

        [TestMethod]
        public void TestMultipleMisMatch()
        {
            TestFindMatch(
                text: "a  b a c a",
                words: "z");
        }

        [TestMethod]
        public void TestMultipleMisMatchWithSpaces()
        {
            TestFindMatch(
                text: "ab cd ab de ad ac bd",
                words: "az");
        }

        [TestMethod]
        public void TestMultipleMisMatchWithWordPrefixSpaces()
        {
            TestFindMatch(
                text: "a  b a c a",
                words: "   z");
        }

        [TestMethod]
        public void TestMultipleMisMatchWithSpacesWithWordTrailingSpaces()
        {
            TestFindMatch(
                text: "ab cd ab de ad ac bd",
                words: "az  ");
        }


        [TestMethod]
        public void TestMultWordsMisMatch()
        {
            TestFindMatch(
                text: "ac bd ac d ac bd ac b a bd c bd c d e a d ac c bd d",
                words: new string[] { "az", "bz" });
        }

        [TestMethod]
        public void TestCannotFindWordsWithSpacesInThem()
        {
            TestFindMatch(
                text: "ac bd ac d ac bd ac b a bd c bd c d e a d ac c bd d",
                words: new string[] { "a z", "b z" });
        }

        public void TestFindMatch(
            string text,
            params string[] words)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 0,
                expectedMatches: 0,
                expectedFuziness: null,
                caseSensitive: true,
                accentSensitive: true);
        }
    }
}
