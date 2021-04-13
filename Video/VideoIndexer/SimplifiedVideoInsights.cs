// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class SimplifiedVideoInsights
    {
        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("keyPhrases")]
        public string[] KeyPhrases { get; set; }

        [JsonProperty("organizations")]
        public string[] Organizations { get; set; }

        [JsonProperty("persons")]
        public string[] Persons { get; set; }

        [JsonProperty("locations")]
        public string[] Locations { get; set; }
        
        [JsonProperty("indexedVideoId")]
        public string IndexedVideoId { get; set; }
        
        [JsonProperty("thumbnailId")]
        public string ThumbnailId { get; set; }
        
        [JsonProperty("originalVideoEncodedMetadataPath")]
        public string OriginalVideoEncodedMetadataPath { get; set; }
        
        [JsonProperty("originalVideoName")]
        public string OriginalVideoName { get; set; }
    }
}