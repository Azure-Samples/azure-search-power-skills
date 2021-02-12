// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Vision.AnalyzeForm
{
    public class Table
    {
        public double[] boundingBox { get; set; }
        public int columns { get; set; }
        public int rows { get; set; }
        public Cell[] cells { get; set; }
    }
}
