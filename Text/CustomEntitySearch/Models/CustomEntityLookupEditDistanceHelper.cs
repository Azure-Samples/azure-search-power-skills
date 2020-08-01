// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public static class CustomEntityLookupEditDistanceHelper
    {
        private const double EditDistanceMismatchPenalty = 1;
        private const double EditDistanceAccentMismatchPenalty = 0.5;

        /// <summary>
        /// Calculate the Demerau Levenshtein Distance between two strings.
        /// If invalid input is provided, this function returns -1
        /// </summary>
        /// <param name="textSubstring">first string</param>
        /// <param name="word">second string</param>
        /// <returns>number of character edits needed to tranform the first string into the second</returns>
        internal static double CalculateDamerauLevenshteinDistance(string textSubstring, string word)
        {
            if (string.IsNullOrEmpty(textSubstring))
            {
                return word?.Length ?? -1;
            }

            if (string.IsNullOrEmpty(word))
            {
                return textSubstring?.Length ?? -1;
            }

            // Note: minor perf improvement could be had by only maintaining a single row of this table
            double[,] dynamicDistanceCalc = new double[textSubstring.Length + 1, word.Length + 1];

            for (int substringIndex = 0; substringIndex <= textSubstring.Length; substringIndex++)
            {
                dynamicDistanceCalc[substringIndex, 0] = substringIndex;
            }
            for (int wordIndex = 0; wordIndex <= word.Length; wordIndex++)
            {
                dynamicDistanceCalc[0, wordIndex] = wordIndex;
            }

            for (int substringIndex = 0; substringIndex < textSubstring.Length; substringIndex++)
            {
                for (int wordIndex = 0; wordIndex < word.Length; wordIndex++)
                {
                    double cost = 0;
                    double additionOrRemovalPenalty = EditDistanceMismatchPenalty;

                    if (textSubstring[substringIndex].Equals(word[wordIndex]))
                    {
                        cost = 0; // the characters match
                    }
                    else if (word[wordIndex].EqualsModuleDiacritics(textSubstring[substringIndex]))
                    {
                        cost = EditDistanceAccentMismatchPenalty; // the characters only differ by accent characters
                    }
                    else
                    {
                        cost = EditDistanceMismatchPenalty; // the characters don't match
                    }

                    if (textSubstring[substringIndex].IsAccent() ^ word[wordIndex].IsAccent())
                    {
                        additionOrRemovalPenalty = EditDistanceAccentMismatchPenalty; // just adding or removing an accent
                    }

                    // Keep the cheapest 
                    dynamicDistanceCalc[substringIndex + 1, wordIndex + 1] =
                        Math.Min(
                            Math.Min(
                                dynamicDistanceCalc[substringIndex, wordIndex + 1] + additionOrRemovalPenalty, // deletion
                                dynamicDistanceCalc[substringIndex + 1, wordIndex] + additionOrRemovalPenalty), // insertion
                                dynamicDistanceCalc[substringIndex, wordIndex] + cost); // substitution

                    if (substringIndex > 0 && wordIndex > 0
                        && textSubstring[substringIndex].Equals(word[wordIndex - 1])
                        && word[wordIndex].Equals(textSubstring[substringIndex - 1]))
                    {
                        dynamicDistanceCalc[substringIndex + 1, wordIndex + 1] =
                            Math.Min(
                                dynamicDistanceCalc[substringIndex + 1, wordIndex + 1],
                                dynamicDistanceCalc[substringIndex - 1, wordIndex - 1] + cost); // transposition
                    }
                }
            }

            return dynamicDistanceCalc[textSubstring.Length, word.Length];
        }

        /// <summary>
        /// Determines if a given character should be considered a "delineating" character
        /// </summary>
        /// <param name="character">the character to evaluate</param>
        /// <returns>true if the character is delineating</returns>
        public static bool IsDelineating(this char character)
        {
            return (Char.IsWhiteSpace(character)
                    || Char.IsSeparator(character)
                    || Char.IsPunctuation(character));
        }

        /// <summary>
        /// Determines if a given character should be considered an accent (diacritic) character
        /// </summary>
        /// <param name="character">the character to evaluate</param>
        /// <returns>true if the character is diacritic</returns>
        public static bool IsAccent(this char character)
        {
            return Char.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark
                   || Char.GetUnicodeCategory(character) == UnicodeCategory.SpacingCombiningMark;
        }

        /// <summary>
        /// Determine if two characters are equal ignoring diacritics
        /// </summary>
        /// <param name="c1">first character</param>
        /// <param name="c2">second character</param>
        /// <returns>true if they're equal ignoring diacritics</returns>
        public static bool EqualsModuleDiacritics(this char c1, char c2)
        {
            return String.Compare(c1.ToString(), c2.ToString(),
                                    CultureInfo.CurrentCulture, CompareOptions.IgnoreNonSpace) == 0;
        }
    }
}
