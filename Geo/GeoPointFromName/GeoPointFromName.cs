// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureCognitiveSearch.PowerSkills.Geo.GeoPointFromName
{
    public static class GeoPointFromName
    {
        private static readonly string azureMapsUri = "https://atlas.microsoft.com/search/fuzzy/json";
        private static readonly string azureMapsKeySetting = "AZUREMAPS_APP_KEY";

        [FunctionName("geo-point-from-name")]
        public static async Task<IActionResult> RunGeoPointFromName(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Geo Point From Name Custom skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            string azureMapsKey = Environment.GetEnvironmentVariable(azureMapsKeySetting, EnvironmentVariableTarget.Process);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var address = inRecord.Data["address"] as string;
                    string uri = azureMapsUri + "?api-version=1.0&query=" + Uri.EscapeDataString(address);

                    IEnumerable<Geography> geographies =
                        await WebApiSkillHelpers.FetchAsync<Geography>(uri, "X-ms-client-id", azureMapsKey, "results");

                    if (geographies.FirstOrDefault() is Geography mainGeoPoint)
                    {
                        outRecord.Data["mainGeoPoint"] = new {
                            Type = "Point",
                            Coordinates = new double[] { mainGeoPoint.Position.Lon, mainGeoPoint.Position.Lat }
                        };
                    }
                    outRecord.Data["results"] = geographies;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
