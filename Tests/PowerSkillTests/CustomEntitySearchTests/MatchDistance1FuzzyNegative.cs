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
    public class MatchDistance1FuzzyNegative : MatchValidationBase
    {
        [TestMethod]
        public void TestSingleCharacterReplace()
        {
            TestFindMatch(
                text: "a",
                words: "z");
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
        public void TestNoDoubleAdd()
        {
            TestFindMatch(
                text: "abcdefg",
                words: "abcde");

            TestFindMatch(
                text: "abcdefg",
                words: "cdefg");

            TestFindMatch(
                text: "abcdefg",
                words: "bcdef");
        }

        [TestMethod]
        public void TestNoDoubleTranspose()
        {
            TestFindMatch(
                text: "abcdefg",
                words: "abcde");

            TestFindMatch(
                text: "abcdefg",
                words: "cdefg");

            TestFindMatch(
                text: "abcdefg",
                words: "bcdef");
        }

        [TestMethod]
        public void TestNoDoubleRemove()
        {
            TestFindMatch(
                text: "cdefg",
                words: "abcdefg");

            TestFindMatch(
                text: "abcde",
                words: "abcdefg");

            TestFindMatch(
                text: "bcdef",
                words: "abcdefg");
        }

        [TestMethod]
        public void TestNoDoubleReplace()
        {
            TestFindMatch(
                text: "zzcdefg",
                words: "abcdefg");

            TestFindMatch(
                text: "abcdezz",
                words: "abcdefg");

            TestFindMatch(
                text: "abzzefg",
                words: "abcdefg");

            TestFindMatch(
                text: "zbcdefz",
                words: "abcdefg");
        }

        [TestMethod]
        public void TestSpaceInWordDontMatch()
        {
            TestFindMatch(
                text: "once upon a time",
                words: "once upon");
        }

        public void TestFindMatch(
            string text,
            string words)
        {
            base.TestFindMatch(
                text: text,
                words: words,
                allowableFuziness: 1,
                expectedMatches: 0,
                expectedFuziness: null,
                caseSensitive: true,
                accentSensitive: true);
        }
    }
}
