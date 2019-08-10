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
        public WordLinker(string executingDirectoryPath)
        {
            string json = File.ReadAllText($"{executingDirectoryPath}\\words.json");
            WordLinker convert = JsonConvert.DeserializeObject<WordLinker>(json);
            Words = convert.Words;
            Synonyms = convert.Synonyms;
            ExactMatch = convert.ExactMatch;
            FuzzyMatchOffset = convert.FuzzyMatchOffset;
            CaseSensitive = convert.CaseSensitive;
        }

        public IList<string> Words
        {
            get; private set;
        }
        public IList<string> ExactMatch
        {
            get; private set;
        }
        public int FuzzyMatchOffset
        {
            get; private set;
        }
        public Dictionary<string, string[]> Synonyms
        {
            get; private set;
        }
        public bool CaseSensitive
        {
            get; private set;
        }
    }
}
