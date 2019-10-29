// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup.Models;

namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntityLookup
{
    /// <summary>
    /// Based on sample custom skill provided in Azure Search. Given a user-defined list of entities,
    /// this function will find all occurences of that entity in some input text.
    /// </summary>
    public static class CustomEntityLookup
    {
        // ** Some global variables used for configuration.**
        // ** Change these values prior to deploying your function **
        public static string EntityDefinitionLocation = "words.csv"; // other option is "words.json"
        public static int DefaultFuzzyEditDistance = 0; // exact matches only by default
        public static bool DefaultCaseSensitive = false; // case insensitive by default
        public static bool DefaultAccentSensitive = false; // ignore differences in character accents by default



        public static CustomEntityLookupImplementation _impl = null;

        /// <summary>
        /// Find instances of custom entities from either words.csv or words.json
        /// in input text
        /// </summary>
        [FunctionName("custom-entity-lookup")]
        public static async Task<IActionResult> RunCustomEntityLookup(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Custom Entity Search function: C# HTTP trigger function processed a request.");

            if (_impl == null || executionContext.FunctionName == "unitTestFunction")
            {
                _impl = new CustomEntityLookupImplementation(CustomEntitiesDefinition.ParseCustomEntityDefinition(EntityDefinitionLocation));
            }

            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{executionContext.FunctionName} - Invalid request record array.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(executionContext.FunctionName, requestRecords,
                 (inRecord, outRecord) =>
                 {
                     if (!inRecord.Data.ContainsKey("text") || inRecord.Data["text"] == null)
                     {
                         outRecord.Errors.Add(new WebApiErrorWarningContract { Message = "Cannot process record without the given key 'text' with a string value" });
                         return outRecord;
                     }

                     string text = inRecord.Data["text"] as string;

                     var foundEntities = _impl.GetCustomLookupResult(text, System.Threading.CancellationToken.None);

                     outRecord.Data["entities"] = foundEntities;
                     return outRecord;
                 });

            return new OkObjectResult(response);
        }

    }

}