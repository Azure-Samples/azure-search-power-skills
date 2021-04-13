// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Template.HelloWorld.VideoIndexerModels
{
    public class Sentiment
    {
        [JsonProperty("sentimentKey")]
        public string SentimentKey { get; set; }
    }
}