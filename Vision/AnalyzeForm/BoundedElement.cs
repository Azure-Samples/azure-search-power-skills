// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Vision.AnalyzeForm
{
    public class BoundedElement
    {
        public string Text { get; set; }
        public double[] BoundingBox { get; set; }

        public double Confidence { get; set; }
    }
}
