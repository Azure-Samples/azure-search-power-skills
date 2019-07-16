// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public class OcrImageMetadata
    {
        public OcrLayoutText HandwrittenLayoutText { get; set; }
        public OcrLayoutText LayoutText { get; set; }
        public string ImageStoreUri { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
