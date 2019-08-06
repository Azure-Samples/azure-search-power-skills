// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public static class HocrGenerator
    {
        [FunctionName("hocr-generator")]
        public static IActionResult RunHocrGenerator(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("hOCR Generator Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null || requestRecords.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    List<OcrImageMetadata> imageMetadataList = ((JArray)inRecord.Data["ocrImageMetadataList"]).ToObject<List<OcrImageMetadata>>();
                    Dictionary<string, string> annotations = ((JArray)inRecord.Data["wordAnnotations"])
                        .Where(o => o.Type != JTokenType.Null)
                        .GroupBy(o => o["value"].Value<string>())
                        .Select(g => g.First())
                        .ToDictionary(o => o["value"].Value<string>(), o => o["description"].Value<string>());

                    List<HocrPage> pages = imageMetadataList
                        .Select((imageMetadata, i) => new HocrPage(imageMetadata, i, annotations))
                        .ToList();

                    outRecord.Data["hocrDocument"] = new HocrDocument(pages);

                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
