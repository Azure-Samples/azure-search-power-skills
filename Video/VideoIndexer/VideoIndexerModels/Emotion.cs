// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels
{
    public class Emotion
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}