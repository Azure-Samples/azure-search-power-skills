// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    public class Entity
    {
        public string Category { get; set; }
        public string Value { get; set; }
        public int Offset { get; set; }
        public double Confidence { get; set; }
    }
}
