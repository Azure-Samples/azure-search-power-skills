// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;


namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class CustomEntity
    {
        [JsonConstructor]
        public CustomEntity(
            string name,
            string description,
            string type,
            string subtype,
            string id,
            bool? caseSensitive,
            bool? accentSensitive,
            int? fuzzyEditDistance,
            bool? defaultCaseSensitive,
            bool? defaultAccentSensitive,
            int? defaultFuzzyEditDistance,
            IList<CustomEntityAlias> aliases)
        {
            Name = name;
            Description = description;
            Type = type;
            Subtype = subtype;
            Id = id;
            CaseSensitive = caseSensitive ?? CustomEntityLookup.DefaultCaseSensitive;
            AccentSensitive = accentSensitive ?? CustomEntityLookup.DefaultAccentSensitive;
            FuzzyEditDistance = fuzzyEditDistance ?? CustomEntityLookup.DefaultFuzzyEditDistance;
            DefaultCaseSensitive = defaultCaseSensitive ?? CustomEntityLookup.DefaultCaseSensitive;
            DefaultAccentSensitive = defaultAccentSensitive ?? CustomEntityLookup.DefaultAccentSensitive;
            DefaultFuzzyEditDistance = defaultFuzzyEditDistance ?? CustomEntityLookup.DefaultFuzzyEditDistance;
            Aliases = aliases ?? new List<CustomEntityAlias>();
        }

        public string Name { get; }

        public string Description { get; }

        public string Type { get; }

        public string Subtype { get; }

        public string Id { get; }

        public bool CaseSensitive { get; }

        public bool AccentSensitive { get; } // Accent sensitivity doesn't work with exact matches (fuzzy 0)

        public int FuzzyEditDistance { get; }

        public bool DefaultCaseSensitive { get; }

        public bool DefaultAccentSensitive { get; } // Accent sensitivity doesn't work with exact matches (fuzzy 0)

        public int DefaultFuzzyEditDistance { get; }

        public IList<CustomEntityAlias> Aliases { get; }
    }
}
