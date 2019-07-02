using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzureCognitiveSearch.PowerSkills.Text.AcronymLinker
{
    class AcronymLinker
    {
        public AcronymLinker(string executingDirectoryPath)
        {
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
