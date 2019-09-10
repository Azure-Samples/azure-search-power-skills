// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class ContractualRule
    {
        public string _type { get; set; }
        public string TargetPropertyName { get; set; }
        public bool MustBeCloseToContent { get; set; }
        public License License { get; set; }
        public string LicenseNotice { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
