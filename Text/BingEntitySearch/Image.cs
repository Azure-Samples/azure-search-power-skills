// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class Image
    {
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public Provider[] Provider { get; set; }
        public string HostPageUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
