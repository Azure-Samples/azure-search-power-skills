// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class BingEntity
    {

        public ContractualRule[] ContractualRules { get; set; }
        public Image Image { get; set; }
        public string Description { get; set; }
        public string BingId { get; set; }
        public string WebSearchUrl { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public EntityPresentationInfo EntityPresentationInfo { get; set; }
    }
}
