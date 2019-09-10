// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.TokenizerTests
{
    [TestClass]
    public class TokenizerTests
    {
        [TestMethod]
        public async Task EmptyTextYieldsNoTokens()
        {
            var words = await Helpers.QuerySkill(
                Text.Tokenizer.Tokenizer.RunTokenizer,
                new { Text = "" },
                "words"
            ) as string[];

            Assert.AreEqual(0, words.Length);
        }

        [TestMethod]
        public async Task TextIsTokenized()
        {
            var words = await Helpers.QuerySkill(
                Text.Tokenizer.Tokenizer.RunTokenizer,
                new { Text = "Text is tokenized" },
                "words"
            ) as string[];

            CollectionAssert.AreEqual(new[] { "text", "tokenized" }, words);
        }

        [TestMethod]
        public async Task StopWordsNumbersAndPunctuationAreRemoved()
        {
            var words = await Helpers.QuerySkill(
                Text.Tokenizer.Tokenizer.RunTokenizer,
                new { Text = "The 11 stop words are going to get removed from this sentence, and the four punctuation signs don't get. in- the; way" },
                "words"
            ) as string[];

            CollectionAssert.AreEqual(new[] { "stop", "words", "going", "removed", "sentence", "punctuation", "signs", "dont", "way" }, words);
        }

        [TestMethod]
        public async Task DuplicateWordsAreIncludedMultipleTimes()
        {
            var words = await Helpers.QuerySkill(
                Text.Tokenizer.Tokenizer.RunTokenizer,
                new { Text = "Redundancy ministry of redundancy suffers from redundancy." },
                "words"
            ) as string[];

            CollectionAssert.AreEqual(new[] { "redundancy", "ministry", "redundancy", "suffers", "redundancy" }, words);
        }

        [TestMethod]
        public async Task DiacriticsAreKeptAndCasingIsNormalizedToLower()
        {
            var words = await Helpers.QuerySkill(
                Text.Tokenizer.Tokenizer.RunTokenizer,
                new {
                    Text = "Mes caractères Accentués sont conservÉs, MÊME en maJusculeS.",
                    LanguageCode = "fr"
                },
                "words"
            ) as string[];

            CollectionAssert.AreEqual(new[] { "caractères", "accentués", "conservés", "majuscules" }, words);
        }
    }
}
