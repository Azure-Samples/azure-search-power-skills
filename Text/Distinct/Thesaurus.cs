// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
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
            Synonyms = new Dictionary<string, string>();
            foreach (IEnumerable<string> lemma in dataset)
            {
                if (!lemma.Any()) continue;
                string canonicalForm = lemma.First();
                foreach (string form in lemma)
                {
                    string normalizedForm = Normalize(form);
                    if (Synonyms.TryGetValue(normalizedForm, out string existingCanonicalForm))
                    {
                        throw new InvalidDataException(
                            $"Thesaurus parsing error: the form '{form}' of the lemma '{canonicalForm}' looks the same, once normalized, as one of the forms of '{existingCanonicalForm}'. Please disambiguate or merge lemmas.");
                    }
                    Synonyms.Add(normalizedForm, canonicalForm);
                }
            }
        }

        public Dictionary<string, string> Synonyms { get; }

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
                .ToLowerInvariant()
                .Where(c => !(char.IsPunctuation(c) || char.IsSeparator(c)))
                .ToArray());
    }
}
