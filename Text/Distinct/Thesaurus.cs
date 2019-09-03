// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.Distinct
{
    public class Thesaurus
    {
        public Thesaurus(string executingDirectoryPath)
            : this(JsonConvert.DeserializeObject<IEnumerable<IEnumerable<string>>>(
                File.ReadAllText($"{executingDirectoryPath}\\thesaurus.json")))
        { }

        public Thesaurus(IEnumerable<IEnumerable<string>> dataset)
        {
            Synonyms = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (IEnumerable<string> lemma in dataset)
            {
                string canonicalForm = lemma.First();
                foreach (string form in lemma)
                {
                    Synonyms.Add(Normalize(form), canonicalForm);
                }
            }
        }

        public Dictionary<string, string> Synonyms
        {
            get; private set;
        }

        public IEnumerable<string> Dedupe(IEnumerable<string> words)
        {
            var normalizedToWord = new Dictionary<string, string>();
            foreach (string word in words)
            {
                string normalized = Normalize(word);
                string canonical = Synonyms.TryGetValue(normalized, out string canonicalFromThesaurus) ?
                    canonicalFromThesaurus :
                    normalized;
                if (!normalizedToWord.ContainsKey(canonical))
                {
                    normalizedToWord.Add(canonical, canonicalFromThesaurus ?? word); // Arbitrarily consider the first occurrence as canonical
                }
            }
            return normalizedToWord.Values.Distinct();
        }

        public static string Normalize(string word)
            => new string(word
                .Normalize()
                .ToLower()
                .Where(c => !(char.IsPunctuation(c) || char.IsSeparator(c)))
                .ToArray());
    }
}
