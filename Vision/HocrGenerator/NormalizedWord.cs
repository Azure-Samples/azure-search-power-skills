// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public class NormalizedWord
    {
        public List<Point> BoundingBox { get; set; }
        public string Text { get; set; }
    }
}
