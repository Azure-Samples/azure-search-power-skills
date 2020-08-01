// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class CustomEntityAlias
    {
        [JsonConstructor]
        public CustomEntityAlias(
            string text,
            bool? caseSensitive,
            bool? accentSensitive,
            int? fuzzyEditDistance)
        {
            Text = text;
            CaseSensitive = caseSensitive;
            AccentSensitive = accentSensitive;
            FuzzyEditDistance = fuzzyEditDistance;
        }

        public string Text { get; }

        public bool? CaseSensitive { get; }

        public bool? AccentSensitive { get; } // Accent sensitivity doesn't work with exact matches (fuzzy 0)

        public int? FuzzyEditDistance { get; }
    }
}
