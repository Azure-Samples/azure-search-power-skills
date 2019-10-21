// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models
{
    public class CustomEntitiesDefinition
    {
        public static CustomEntitiesDefinition ParseCustomEntityDefinition(string fileName)
        {
            var local_root = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azure_root = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var actual_root = local_root ?? azure_root;

            if (fileName.EndsWith(".json"))
            {
                string json = File.ReadAllText(Path.Join(actual_root, fileName));
                var entities = JsonConvert.DeserializeObject<List<CustomEntity>>(json);
                return new CustomEntitiesDefinition(entities);
            }
            else if (fileName.EndsWith(".csv"))
            {
                return new CustomEntitiesDefinition(
                    targetCustomEntities: 
                        File.ReadAllLines(Path.Join(actual_root, fileName))
                            .SelectMany(line => line.Split(","))
                            .Where(line => !string.IsNullOrEmpty(line))
                            .Select(s => new CustomEntity(s, null, null, null, null, null, null, null, null, null, null, null))
                            .ToList()
                );
            }
            else
            {
                throw new ArgumentException("Unsupported Entity Definition file type.");
            }
        }

        [JsonConstructor]
        public CustomEntitiesDefinition(
            IList<CustomEntity> targetCustomEntities)
        {
            TargetCustomEntities = targetCustomEntities ?? new List<CustomEntity>();
        }

        public IList<CustomEntity> TargetCustomEntities
        {
            get;
        }
    }
}
