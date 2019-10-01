// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    internal class WordLinker
    {
        public static WordLinker WordLink(string fileName)
        {
            var local_root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azure_root = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var actual_root = local_root ?? azure_root;

            if (fileName.EndsWith(".json"))
            {
                string json = File.ReadAllText(Path.Join(actual_root, fileName));
                return JsonConvert.DeserializeObject<WordLinker>(json);
            }
            else if (fileName.EndsWith(".csv"))
            {
                return new WordLinker
                {
                    Words = File.ReadAllLines(Path.Join(actual_root, fileName))
                            .SelectMany(line => line.Split(","))
                            .Where(line => !string.IsNullOrEmpty(line))
                            .ToList()
                };
            }
            else
            {
                throw new ArgumentException("Unsupported Entity Definition file type.");
            }
        }

        public IList<string> Words
        {
            get; set;
        }

        public IList<string> ExactMatch
        {
            get; set;
        }

        /// <summary>
        /// The amount of lenincy the mathcing algorithm will allow in a match.
        /// This is based on Levenshtein distance. EG: cat - catt have a distance of 1
        /// </summary>
        public int FuzzyEditDistance
        {
            get; set;
        }

        public Dictionary<string, string[]> Synonyms
        {
            get; set;
        }

        public bool CaseSensitive
        {
            get; set;
        }
    }
}
