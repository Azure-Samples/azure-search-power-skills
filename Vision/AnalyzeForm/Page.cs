// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Vision.AnalyzeForm
{
    public class Page
    {
        [JsonProperty(PropertyName ="page")]
        public int Number { get; set; }
        public int? ClusterId { get; set; }

        public KeyValuePair[] KeyValuePairs { get; set; }
    }
}
