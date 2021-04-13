// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Template.HelloWorld
{
    public static class VideoIndexer
    {
        [FunctionName("video-indexer")]
        public static async Task<IActionResult> RunVideoIndexer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Video Indexer Custom Skill: C# HTTP trigger function processed a request");

            var skillName = executionContext.FunctionName;
            var requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            var response = await WebApiSkillHelpers.ProcessRequestRecordsAsync(skillName,
                requestRecords,
                async (inRecord, outRecord) =>
                {
                    var encodedVideoUrl = (string)inRecord.Data["metadata_storage_path"];
                    var videoUrl = UrlSafeBase64Decode(encodedVideoUrl);
                    var videoName = (string)inRecord.Data["metadata_storage_name"];

                    var client = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerStorageConnectionStringAppSetting));
                    var blob = await client.CreateCloudBlobClient().GetBlobReferenceFromServerAsync(new Uri(videoUrl));
                    var sasKey = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
                    {
                        Permissions = SharedAccessBlobPermissions.Read,
                        SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(10)
                    });
                    
                    await InitiateVideoIndexing(log, encodedVideoUrl, videoUrl + sasKey, videoName);
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        private static async Task InitiateVideoIndexing(ILogger logger, string encodedVideoUrl, string videoUrl, string videoName)
        {
            var endpoint = "https://api.videoindexer.ai";
            
            var accountId = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountIdAppSetting);
            var location = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerLocationAppSetting);
            var videoIndexerAccountKey  = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountKeyAppSetting);
            var functionCode = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerCallbackFunctionCodeAppSetting);
            var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            var callbackUrl = $"https://{hostName}/api/video-indexer-callback?code={functionCode}&encodedPath={encodedVideoUrl}";
            logger.LogInformation("Passing callback to indexer at: {CallbackUrl}", callbackUrl.Replace(functionCode, functionCode.Substring(0, 5) + "XXX"));
            var privacy = "Private";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", videoIndexerAccountKey);

            var accessToken = JsonConvert.DeserializeObject<string>(await httpClient.GetStringAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true"));
            
            logger.LogInformation("Retrieved access token for Video Indexer");

            videoUrl = Uri.EscapeDataString(videoUrl);

            var response = await httpClient.PostAsync(
                $"{endpoint}/{location}/Accounts/{accountId}/Videos?accessToken={accessToken}&name={Uri.EscapeDataString(videoName)}&videoUrl={videoUrl}&privacy={privacy}&callbackUrl={Uri.EscapeDataString(callbackUrl)}",
                new MultipartFormDataContent());

            logger.LogInformation("Submitted video {VideoName} for indexing", videoName);

            response.EnsureSuccessStatusCode();
        }
        
        private static string UrlSafeBase64Decode(string encoded)
        {
            encoded = encoded
                .Replace("-", "+")
                .Replace("_", "/");

            if (encoded.EndsWith("1")) encoded = encoded.Substring(0, encoded.Length - 1) + "=";
            if (encoded.EndsWith("2")) encoded = encoded.Substring(0, encoded.Length - 1) + "==";

            return Encoding.Default.GetString(Convert.FromBase64String(encoded));
        }

    }
}