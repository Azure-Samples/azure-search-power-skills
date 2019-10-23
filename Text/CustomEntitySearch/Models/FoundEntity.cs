// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class FoundEntity
    {
        [JsonConstructor]
        public FoundEntity(
            string name,
            string description,
            string id,
            string type,
            string subtype,
            IList<FoundEntityMatchDetails> matches)
        {
            Name = name;
            Description = description;
            Id = id;
            Type = type;
            Subtype = subtype;
            Matches = matches ?? new List<FoundEntityMatchDetails>();
        }

        public string Name { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Subtype { get; set; }

        public IList<FoundEntityMatchDetails> Matches { get; set; }
    }
}
