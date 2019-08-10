// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Vision.ImageStore
{
    public static class ImageStore
    {
        private static readonly string blobStorageConnectionStringSetting = "BLOB_STORAGE_CONNECTION_STRING";
        private static readonly string blobStorageContainerNameSetting = "BLOB_STORAGE_CONTAINER_NAME";
        private static readonly string blobContainerHeaderName = "BlobContainerName";

        [FunctionName("image-store")]
        public static async Task<IActionResult> RunStoreImage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Image Store Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null || requestRecords.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }

            string blobStorageConnectionString = Environment.GetEnvironmentVariable(blobStorageConnectionStringSetting, EnvironmentVariableTarget.Process);
            string blobContainerName = String.IsNullOrEmpty(req.Headers[blobContainerHeaderName])
                ? Environment.GetEnvironmentVariable(blobStorageContainerNameSetting, EnvironmentVariableTarget.Process)
                : (string)req.Headers[blobContainerHeaderName];

            if (String.IsNullOrEmpty(blobStorageConnectionString) || String.IsNullOrEmpty(blobContainerName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the blob storage account is missing");
            }
            var imageStore = new BlobImageStore(blobStorageConnectionString, blobContainerName);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var imageData = inRecord.Data["imageData"] as string;
                    var imageName = inRecord.Data["imageName"] as string;
                    if (String.IsNullOrEmpty(imageName))
                    {
                        imageName = Guid.NewGuid().ToString();
                    }
                    var mimeType = inRecord.Data["mimeType"] as string;
                    if (String.IsNullOrEmpty(mimeType))
                    {
                        mimeType = "image/jpeg";
                    }
                    string imageUri = await imageStore.UploadToBlobAsync(imageData, imageName, mimeType);
                    outRecord.Data["imageStoreUri"] = imageUri;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        [FunctionName("image-fetch")]
        public static async Task<IActionResult> RunFetchImage(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Image Fetch Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null || requestRecords.Count() != 1)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array: Skill requires exactly 1 image per request.");
            }

            string blobStorageConnectionString = Environment.GetEnvironmentVariable(blobStorageConnectionStringSetting, EnvironmentVariableTarget.Process);
            string blobContainerName = String.IsNullOrEmpty(req.Headers[blobContainerHeaderName])
                ? Environment.GetEnvironmentVariable(blobStorageContainerNameSetting, EnvironmentVariableTarget.Process)
                : (string)req.Headers[blobContainerHeaderName];

            if (String.IsNullOrEmpty(blobStorageConnectionString) || String.IsNullOrEmpty(blobContainerName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the blob storage account is missing");
            }
            var imageStore = new BlobImageStore(blobStorageConnectionString, blobContainerName);

            WebApiSkillResponse response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName, requestRecords,
                async (inRecord, outRecord) => {
                    var imageUri = inRecord.Data["imageStoreUri"] as string;
                    if (!String.IsNullOrEmpty(imageUri))
                    {
                        Image image = await imageStore.DownloadFromBlobAsync(imageUri);
                        outRecord.Data["imageName"] = image.Name;
                        outRecord.Data["mimeType"] = image.MimeType;
                        outRecord.Data["imageData"] = image.Data;
                    }
                    return outRecord;
                });

            return new OkObjectResult(response);
        }
    }
}
