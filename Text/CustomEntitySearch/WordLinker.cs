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
            string json = File.ReadAllText($"{executingDirectoryPath}\\Users\\t-neja\\azure-search-power-skills\\Text\\CustomEntitySearch\\words.json");
            Value convert = JsonConvert.DeserializeObject<Value>(json);
            Words = convert.Words;
            Synonyms = convert.Synonyms;
            ExactMatch = convert.ExactMatch;
            FuzzyMatchOffset = convert.FuzzyMatchOffset;
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

        private class Value
        {
            public IList<string> Words { get; set; }
            public Dictionary<string, string[]> Synonyms { get; set; }
            public IList<string> ExactMatch { get; set; }
            public int FuzzyMatchOffset { get; set; }
        }
    }
}
