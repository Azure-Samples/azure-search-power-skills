// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class CustomEntitySelection
    {
        /// <summary>
        /// Create a selection from the base custom entity
        /// </summary>
        public CustomEntitySelection(CustomEntity entity)
            : this(
                  entity.Name,
                  entity.CaseSensitive,
                  entity.AccentSensitive,
                  entity.FuzzyEditDistance,
                  entity)
        {
        }

        public CustomEntitySelection(CustomEntity entity, CustomEntityAlias alias)
            : this(
                  alias.Text,
                  alias.CaseSensitive,
                  alias.AccentSensitive,
                  alias.FuzzyEditDistance,
                  entity)
        {
        }

        public CustomEntitySelection(
            string text,
            bool? caseSensitive,
            bool? accentSensitive,
            int? fuzzyEditDistance,
            CustomEntity parentEntityReference)
        {
            Text = text;
            CaseSensitive = caseSensitive ?? parentEntityReference.DefaultCaseSensitive;
            AccentSensitive = accentSensitive ?? parentEntityReference.DefaultCaseSensitive;
            FuzzyEditDistance = fuzzyEditDistance ?? parentEntityReference.DefaultFuzzyEditDistance;
            ParentEntityReference = parentEntityReference;
        }

        public string Text { get; }

        public bool CaseSensitive { get; }

        public bool AccentSensitive { get; }

        public int FuzzyEditDistance { get; }

        public CustomEntity ParentEntityReference { get; }

        public string NormalizedText
        {
            get
            {
                if (_normalizedText != null)
                {
                    return _normalizedText;
                }

                _normalizedText = CustomEntityLookupImplementation.NormalizeWord(Text, CaseSensitive, AccentSensitive);
                return _normalizedText;
            }
        }
        private string _normalizedText;

        public override int GetHashCode()
        {
            // TODO: this should be reflective of a "unique" custom entity
            return base.GetHashCode();
        }
    }
}
