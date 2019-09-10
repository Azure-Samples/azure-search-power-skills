using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    public class PotentialEntity
    {
        private static int whitespaceIndex = -1;
        private static int startIndex = 0;
        private static int endIndex = 0;
        private static StringWriter matchedWordInText = new StringWriter();
        private static HashSet<char> wordDiff = new HashSet<char>();
        private static HashSet<char> textDiff = new HashSet<char>();
        public static double mismatchScore = 0.0;

        public static int GetStartIndex()
        {
            return startIndex;
        }
        public static int GetEndIndex()
        {
            return endIndex;
        }
        public static string GetMatch(int whitespaceCheckOverride = 0)
        {
            if (whitespaceCheckOverride < matchedWordInText.ToString().Length && whitespaceCheckOverride > 0)
            {
                return matchedWordInText.ToString().Substring(0, matchedWordInText.ToString().Length - whitespaceCheckOverride);
            }
            return matchedWordInText.ToString();
        }

        public static void ResetPotentialEntity(int shifter = 1)
        {
            if (whitespaceIndex == -1)
                endIndex += shifter;
            else
                endIndex = whitespaceIndex;
            whitespaceIndex = -1;
            startIndex = endIndex;
            mismatchScore = 0.0;
            matchedWordInText.GetStringBuilder().Clear();
            wordDiff.Clear();
            textDiff.Clear();
        }

        public static void MatchInText(char addToEntity, double addToMismatch = 0.0)
        {
            if (addToEntity.IsDelineating() && whitespaceIndex == -1)
                whitespaceIndex = endIndex;
            mismatchScore += addToMismatch;
            matchedWordInText.Write(addToEntity);
            endIndex++;
        }

        public static void AddToDiff(char wordChar, char textChar)
        {
            wordDiff.Add(wordChar);
            textDiff.Add(textChar);
        }

        public static int CheckDiff()
        {
            return wordDiff.SetEquals(textDiff) ? (wordDiff.Count + (wordDiff.Count % 2)) / 2  : 0;
        }

        public static void Clear()
        {
            startIndex = 0;
            endIndex = 0;
            whitespaceIndex = -1;
            matchedWordInText.GetStringBuilder().Clear();
            mismatchScore = 0.0;
            wordDiff.Clear();
            textDiff.Clear();
        }
    }
}
