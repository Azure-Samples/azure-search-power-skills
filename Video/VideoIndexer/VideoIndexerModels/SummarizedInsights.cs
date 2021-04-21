// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels
{
    public class SummarizedInsights
    {
        [JsonProperty("faces")]
        public Face[] Faces { get; set; }

        [JsonProperty("keywords")]
        public Keyword[] Keywords { get; set; }

        [JsonProperty("sentiments")]
        public Sentiment[] Sentiments { get; set; }

        [JsonProperty("emotions")]
        public Emotion[] Emotions { get; set; }

        [JsonProperty("labels")]
        public Label[] Labels { get; set; }

        [JsonProperty("namedLocations")]
        public NamedLocation[] NamedLocations { get; set; }

        [JsonProperty("topics")]
        public Topic[] Topics { get; set; }

        [JsonProperty("thumbnailId")]
        public string ThumbnailId { get; set; }
    }
}