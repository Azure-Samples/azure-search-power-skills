// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Text;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class VideoIndexer
    {
        private readonly IVideoIndexerBlobClient _videoIndexerBlobClient;
        private readonly IVideoIndexerClient _videoIndexerClient;

        public VideoIndexer(IVideoIndexerBlobClient videoIndexerBlobClient,  IVideoIndexerClient videoIndexerClient)
        {
            this._videoIndexerBlobClient = videoIndexerBlobClient;
            this._videoIndexerClient = videoIndexerClient;
        }
        
        [FunctionName("video-indexer")]
        public async Task<IActionResult> RunVideoIndexer(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
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
                    var sasKey = await _videoIndexerBlobClient.GetSasKey(videoUrl);
                    var videoId = await _videoIndexerClient.SubmitVideoIndexingJob( videoUrl + sasKey,  encodedVideoUrl, videoName);
                    log.LogInformation("Uploaded video {VideoName} - Video Id in indexer: {Id}", videoName, videoId);
                    outRecord.Data["videoId"] = videoId;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }


        private static string UrlSafeBase64Decode(string encoded)
        {
            encoded = encoded
                .Replace("-", "+")
                .Replace("_", "/");

            if (encoded.EndsWith("0")) encoded = encoded.Substring(0, encoded.Length - 1);
            if (encoded.EndsWith("1")) encoded = encoded.Substring(0, encoded.Length - 1) + "=";
            if (encoded.EndsWith("2")) encoded = encoded.Substring(0, encoded.Length - 1) + "==";

            return Encoding.Default.GetString(Convert.FromBase64String(encoded));
        }

    }
}