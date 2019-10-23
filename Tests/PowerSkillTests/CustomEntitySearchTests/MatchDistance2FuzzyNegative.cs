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
    public class MatchDistance2FuzzyNegative :MatchValidationBase
    {
        [TestMethod]
        public void TestSingleCharacterReplace()
        {
            TestFindMatch(
                text: "a",
                words: "z");
        }

        [TestMethod]
        public void TestDoubleCharacterRemove()
        {
            TestFindMatch(
                text: "a",
                words: "bc");
        }

        [TestMethod]
        public void TestSingleCharacterWithSpacesReplace()
        {
            TestFindMatch(
                text: " a",
                words: "z");

            TestFindMatch(
                text: "a ",
                words: "z");

            TestFindMatch(
                text: " a ",
                words: "z");
        }

        [TestMethod]
        public void TestNoNestedTransposition()
        {
            TestFindMatch(
                text: " abcd",
                words: "cad");

            TestFindMatch(
                text: "abcd ",
                words: "cad");

            TestFindMatch(
                text: "  abcd  ",
                words: "cad");
        }

        [TestMethod]
        public void TestNoTripleAdd()
        {
            TestFindMatch(
                text: "abcdefg",
                words: "abcd");

            TestFindMatch(
                text: "abcdefg",
                words: "defg");

            TestFindMatch(
                text: "abcdefg",
                words: "bcef");
        }

        [TestMethod]
        public void TestNoTripleTranspose()
        {
            TestFindMatch(
                text: "abcdef",
                words: "badcfe");

            TestFindMatch(
                text: "abcdefg",
                words: "badcfe");

            TestFindMatch(
                text: "abcdefg",
                words: "acbedgf");
        }


        [TestMethod]
        public void TestNoTripleRemove()
        {
            TestFindMatch(
                text: "defg",
                words: "abcdefg");

            TestFindMatch(
                text: "abcd",
                words: "abcdefg");

            TestFindMatch(
                text: "bcef",
                words: "abcdefg");
        }

        [TestMethod]
        public void TestNoTripleReplace()
        {
            TestFindMatch(
                text: "zzzcdefg",
                words: "abcdefg");

            TestFindMatch(
                text: "abcdezzz",
                words: "abcdefg");

            TestFindMatch(
                text: "abzzzefg",
                words: "abcdefg");

            TestFindMatch(
                text: "zbczdefz",
                words: "abcdefg");
        }

        [TestMethod]
        public void TestNoTripleComposedEdits()
        {
            TestFindMatch(
                text: "abcdefg",
                words: "azcedf");

            TestFindMatch(
                text: "abcdefg",
                words: "badegf");

            TestFindMatch(
                text: "abcdefg",
                words: "zzabdcefg");

            TestFindMatch(
                text: "abcdefg",
                words: "cdfeg");
        }

        [TestMethod]
        public void TestWordWithSpacesDoesNotMatch()
        {
            TestFindMatch(
                text: "once upon a time",
                words: "once upon");
        }

        public void TestFindMatch(
            string text,
            string words,
            double? expectedFuziness = null)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 2,
                expectedMatches: 0,
                expectedFuziness: null,
                caseSensitive: true,
                accentSensitive: true);
        }
    }
}
