// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    internal class WordLinker
    {
        public WordLinker(string fileType)
        {
            var local_root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azure_root = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var actual_root = local_root ?? azure_root;

            if (fileType == "json")
            {
                string json = File.ReadAllText($"{actual_root}\\words.json");
                Words = new List<string>(JsonConvert.DeserializeObject<List<string>>(json));
            }
            else if (fileType == "csv")
            {
                Words = File.ReadAllLines($"{actual_root}\\words.csv").ToList();
            }

        }

        public IList<string> Words
        {
            get; private set;
        }
    }

}
