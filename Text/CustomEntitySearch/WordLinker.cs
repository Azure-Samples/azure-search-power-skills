using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    class WordsConfig
    {
        public List<string> TargetWords { get; set; }
    }
    class WordLinker
    {
        public WordLinker(string executingDirectoryPath)
        {
            string json = File.ReadAllText($"{executingDirectoryPath}\\words.json");
            Words = new List<string>(
                JsonConvert.DeserializeObject<WordsConfig>(json).TargetWords);
        }

        public List<string> Words
        {
            get; private set;
        }
    }
}
