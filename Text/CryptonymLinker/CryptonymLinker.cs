using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzureCognitiveSearch.PowerSkills.Text.CryptonymLinker
{
    class CryptonymLinker
    {
        public CryptonymLinker(string executingDirectoryPath)
        {
            string json = File.ReadAllText($"{executingDirectoryPath}\\acronyms.json");
            Cryptonyms = new Dictionary<string, string>(
                JsonConvert.DeserializeObject<Dictionary<string, string>>(json),
                StringComparer.InvariantCultureIgnoreCase);
        }

        public Dictionary<string, string> Cryptonyms
        {
            get; private set;
        }
    }
}
