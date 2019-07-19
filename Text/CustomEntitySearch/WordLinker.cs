using Newtonsoft.Json;
using System;
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
            Words = new List<string>(
                JsonConvert.DeserializeObject<List<string>>(json));
        }

        public IList<string> Words
        {
            get; private set;
        }
    }
}
