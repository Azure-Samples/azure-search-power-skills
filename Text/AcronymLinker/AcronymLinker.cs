// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzureCognitiveSearch.PowerSkills.Text.AcronymLinker
{
    public class AcronymLinker
    {
        public static Dictionary<string, string> TestDataSet { get; set; }

        public AcronymLinker(string executingDirectoryPath)
        {
            if (TestDataSet != null)
            {
                Acronyms = TestDataSet;
                return;
            }
            string json = File.ReadAllText($"{executingDirectoryPath}\\acronyms.json");
            Acronyms = new Dictionary<string, string>(
                JsonConvert.DeserializeObject<Dictionary<string, string>>(json),
                StringComparer.InvariantCultureIgnoreCase);
        }

        public Dictionary<string, string> Acronyms
        {
            get; private set;
        }
    }
}
