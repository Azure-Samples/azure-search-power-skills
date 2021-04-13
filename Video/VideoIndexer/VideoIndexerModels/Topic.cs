// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels
{
    public class Topic
    {
        [JsonProperty("confidence")]
        public double Confidence { get; set; }
        
        [JsonProperty("iabName")]
        public string IabName { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}