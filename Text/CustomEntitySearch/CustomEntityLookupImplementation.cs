// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup
{
    public class CustomEntityLookupImplementation
    {
        public const int MaxAllowableFuziness = 5;
        private const int MaxRegexEvalTimeInSeconds = 30;
        private const int MaxNumberOfWordsInEntity = 5;

        public const string ErrorParsingDefinitionFormat = "Failed to either download or parse Custom Entity Definition file.";
        public const string ErrorCustomLookupSearchPerformance = "An error occurred while trying to find matches in your document: {0}";
        public const string ExceededMatchCapacityWarning = "Reached maximum memory capacity for matches, skipping all further duplicate matches.";
        public const string DuplicateLookupTermWarningFormat = "Encountered duplicate lookup term '{0}'. The skill will only find matches for the first instance of this term.";

        private readonly CustomEntitiesDefinition _targetEntities;
        private readonly HashSet<CustomEntitySelection> _selections;

        public CustomEntityLookupImplementation(CustomEntitiesDefinition entityDefinitions)
        {
            _targetEntities = entityDefinitions;
            _selections = new HashSet<CustomEntitySelection>();

            foreach (var tce in _targetEntities.TargetCustomEntities)
            {
                var entitySelection = new CustomEntitySelection(tce);
                _selections.Add(entitySelection);

                if (tce.Aliases?.Any() == true)
                {
                    foreach (var alias in tce.Aliases)
                    {
                        var aliasSelection = new CustomEntitySelection(tce, alias);

                        if (!_selections.Contains(aliasSelection))
                        {
                            _selections.Add(aliasSelection);
                        }
                    }
                }
            }
        }
        public IList<FoundEntity> GetCustomLookupResult(
            string inputText,
            CancellationToken cancellationToken)
        {
            inputText = TrimDelineatingCharacters(inputText);
            var entitiesFound = new Dictionary<CustomEntity, FoundEntity>(_targetEntities.TargetCustomEntities.Count);

            // Find exact matches with Regex
            var exactSelections = _selections.Where(s => s.FuzzyEditDistance <= 0).ToList();
            foreach (var selection in exactSelections)
            {
                var pattern = BuildRegexForExactMatch(selection);
                MatchCollection entityMatch = Regex.Matches(inputText, pattern, RegexOptions.None, TimeSpan.FromSeconds(MaxRegexEvalTimeInSeconds));
                if (entityMatch.Count != 0)
                {
                    entitiesFound[selection.ParentEntityReference] = new FoundEntity(
                        name: selection.ParentEntityReference.Name,
                        description: selection.ParentEntityReference.Description,
                        id: selection.ParentEntityReference.Id,
                        type: selection.ParentEntityReference.Type,
                        subtype: selection.ParentEntityReference.Subtype,
                        matches: new List<FoundEntityMatchDetails>());

                    foreach (Match match in entityMatch)
                    {
                        entitiesFound[selection.ParentEntityReference].Matches.Add(
                            new FoundEntityMatchDetails(
                                text: match.Groups[1].Value,
                                offset: match.Index,
                                length: match.Groups[1].Value.Length,
                                matchDistance: 0));
                    }
                }
            }


            // Find fuzzy matches with naive Levenstein comparisons
            var substrings = TokenizeInputText(inputText);
            var fuzzySelections = _selections.Where(s => s.FuzzyEditDistance > 0).ToList();
            if (fuzzySelections.Any())
            {
                foreach ((string substring, int startIndex) in substrings)
                {
                    foreach (var selection in fuzzySelections)
                    {
                        var normalizedSubstring = NormalizeWord(substring, selection.CaseSensitive, selection.AccentSensitive);
                        var normalizedWord = selection.NormalizedText;
                        var editDistance = CustomEntityLookupEditDistanceHelper.CalculateDamerauLevenshteinDistance(normalizedSubstring, normalizedWord);
                        var fuzziness = selection.FuzzyEditDistance >= normalizedWord.Length ? normalizedWord.Length - 1 : selection.FuzzyEditDistance;
                        fuzziness = Math.Max(0, fuzziness);
                        if (editDistance <= fuzziness)
                        {
                            if (!entitiesFound.ContainsKey(selection.ParentEntityReference))
                            {
                                entitiesFound[selection.ParentEntityReference] = new FoundEntity(
                                    name: selection.ParentEntityReference.Name,
                                    description: selection.ParentEntityReference.Description,
                                    id: selection.ParentEntityReference.Id,
                                    type: selection.ParentEntityReference.Type,
                                    subtype: selection.ParentEntityReference.Subtype,
                                    matches: new List<FoundEntityMatchDetails>());
                            }

                            entitiesFound[selection.ParentEntityReference].Matches.Add(
                                new FoundEntityMatchDetails(
                                    text: substring,
                                    offset: startIndex,
                                    length: substring.Length,
                                    matchDistance: editDistance));
                        }
                    }
                }
            }

            return entitiesFound.Values.ToList();
        }

        private string BuildRegexForExactMatch(CustomEntitySelection selection)
        {
            string word = selection.Text;
            StringBuilder escapedWord = new StringBuilder(@"(?=(");
            if (!word.First().IsDelineating())
            {
                escapedWord.Append(@"\b");
            }

            for (int currWordCharIndex = 0; currWordCharIndex < word.Length; currWordCharIndex++)
            {
                if (word[currWordCharIndex].IsDelineating())
                {
                    escapedWord.Append(@"[\s\p{P}]");
                }
                else
                {
                    escapedWord.Append(word[currWordCharIndex]);
                }
            }
            if (!word.Last().IsDelineating())
            {
                escapedWord.Append(@"\b");
            }

            escapedWord.Append("))");
            string pattern = (selection.CaseSensitive) ? @"(?x)" + escapedWord : @"(?ix)" + escapedWord;

            return pattern;
        }

        public static string NormalizeWord(
            string initialWord,
            bool caseSensitive,
            bool accentSensitive)
        {
            if (!caseSensitive)
            {
                initialWord = initialWord.ToLowerInvariant();
            }

            if (!accentSensitive)
            {
                initialWord = StripDiacritics(initialWord);
            }

            initialWord = TrimDelineatingCharacters(initialWord);

            return initialWord;
        }

        /// <summary>
        /// Trims delineating characters from the beginning and end of some text
        /// </summary>
        /// <param name="textToTrim">the text to trim</param>
        /// <returns>a string with the delineating characters trimmed from the beginning and end</returns>
        public static string TrimDelineatingCharacters(string textToTrim)
        {
            int startIndex = 0;
            int endIndex = textToTrim.Length - 1;

            while (startIndex < textToTrim.Length && textToTrim[startIndex].IsDelineating())
            {
                startIndex++;
            }

            while (endIndex >= 0 && textToTrim[endIndex].IsDelineating())
            {
                endIndex--;
            }

            if (startIndex >= endIndex + 1)
            {
                return string.Empty;
            }

            return textToTrim.Substring(startIndex, endIndex - startIndex + 1);
        }

        private static string StripDiacritics(string originalString)
        {
            int originalStringLength = originalString.Length;
            char[] newString = new char[originalStringLength];
            int newStringLength = 0;
            for (int i = 0; i < originalStringLength; i++)
            {
                char c = originalString[i];
                if (c.IsAccent())
                {
                    continue;
                }
                newString[newStringLength++] = c;
            }
            return new String(newString, 0, newStringLength);
        }

        private IEnumerable<(string, int)> TokenizeInputText(string text)
        {
            // NOTE: swap me out for new, improved tokenizers!

            int startIndex = 0;
            int endIndex = 0;
            string substring = null;


            if (text[0].IsDelineating())
            {
                startIndex = FindNextStartPointer(text, -1);
            }


            while (startIndex < text.Length)
            {
                endIndex = FindNextEndPointer(text, endIndex);

                var compoundEndIndex = endIndex;
                for (int i = 0; i < MaxNumberOfWordsInEntity; i++)
                {
                    substring = text.Substring(startIndex, compoundEndIndex - startIndex);

                    if (!string.IsNullOrWhiteSpace(substring))
                    {
                        yield return (substring, startIndex);
                    }

                    var previousEndIndex = compoundEndIndex;
                    compoundEndIndex = FindNextEndPointer(text, compoundEndIndex);

                    if (compoundEndIndex == previousEndIndex)
                    {
                        // don't return end words many times
                        break;
                    }
                }

                startIndex = FindNextStartPointer(text, startIndex);
            }
        }

        private int FindNextStartPointer(string text, int currentStart)
        {
            for (int currTextIndex = currentStart + 1; currTextIndex < text.Length; currTextIndex++)
            {
                if (text[currTextIndex].IsDelineating())
                {
                    if (currTextIndex + 1 < text.Length && !text[currTextIndex + 1].IsDelineating())
                    {
                        return currTextIndex + 1;
                    }
                }
            }

            return text.Length;
        }

        private int FindNextEndPointer(string text, int currentEnd)
        {
            for (int currTextIndex = currentEnd + 1; currTextIndex < text.Length; currTextIndex++)
            {
                if (text[currTextIndex].IsDelineating())
                {
                    if (currTextIndex - 1 >= 0 && !text[currTextIndex - 1].IsDelineating())
                    {
                        return currTextIndex;
                    }
                }
            }

            return text.Length;
        }
    }


}
