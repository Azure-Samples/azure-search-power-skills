// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels
{
    public class VideoIndexerResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("summarizedInsights")]
        public SummarizedInsights SummarizedInsights { get; set; }
        
        [JsonProperty("videos")]
        public Video[] Videos { get; set; }
    }
}