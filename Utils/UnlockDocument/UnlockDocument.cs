// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace AzureCognitiveSearch.PowerSkills.Utils.UnlockDocument
{
    public static class UnlockDocument
    {
        [FunctionName("unlock-document")]
        public static IActionResult RunUnlockDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Unlock Document Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            // Set up access to keyvault to retrieve the key to decrypt the document with
            // Requires that this Azure Function has access via managed identity to the Keyvault where the key is stored.
            var azureServiceTokenProvider1 = new AzureServiceTokenProvider();
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider1.KeyVaultTokenCallback));
            KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(kv);

            // Set up access to blob storage account where the file lives and is encrypted
            // Requires that the Azure Function has application settings for storageAccountName and storageAccountKey
            StorageCredentials creds = new StorageCredentials(
                GetAppSetting("storageAccountName"),
                GetAppSetting("storageAccountKey")
            );
            CloudStorageAccount account = new CloudStorageAccount(creds, useHttps: true);
            CloudBlobClient client = account.CreateCloudBlobClient();
            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(null, cloudResolver);
            BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) =>
                {
                    string blobPath = (string)inRecord.Data["metadata_storage_path"];
                    log.LogInformation(blobPath);
                    var blob = client.GetBlobReferenceFromServer(new Uri(blobPath));
                    byte[] unlockedFileData;
                    using (var np = new MemoryStream())
                    {
                        blob.DownloadToStream(np, null, options, null);
                        unlockedFileData = np.ToArray();
                    }
                    var unlockedFileReference = new FileReference()
                    {
                        data = Convert.ToBase64String(unlockedFileData)
                    };
                    JObject jObject = JObject.FromObject(unlockedFileReference);
                    jObject["$type"] = "file";
                    outRecord.Data["unlocked_file_data"] = jObject;
                    return outRecord;
                });
            return new OkObjectResult(response);
        }

        private static string GetAppSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
