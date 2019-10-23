// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class FoundEntityMatchDetails
    {
        [JsonConstructor]
        public FoundEntityMatchDetails(
            string text,
            int offset,
            int length,
            double matchDistance)
        {
            Text = text;
            Offset = offset;
            Length = length;
            MatchDistance = matchDistance;
        }

        public string Text { get; }

        public int Offset { get; }

        public int Length { get; }

        public double MatchDistance { get; }
    }
}
