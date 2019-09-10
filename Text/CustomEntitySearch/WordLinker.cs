// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    internal class WordLinker
    {
        public static WordLinker WordLink(string executingDirectoryPath)
        {
            string json = File.ReadAllText($"{executingDirectoryPath}\\words.json");
            return JsonConvert.DeserializeObject<WordLinker>(json);
        }

        public IList<string> Words
        {
            get;
        }
        public IList<string> ExactMatch
        {
            get;
        }
        public int FuzzyMatchOffset
        {
            get;
        }
        public Dictionary<string, string[]> Synonyms
        {
            get;
        }
        public bool CaseSensitive
        {
            get;
        }
    }
}
