// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class VideoIndexingCallback
    {
        private readonly IVideoIndexerClient _videoIndexerClient;

        public VideoIndexingCallback(IVideoIndexerClient videoIndexerClient)
        {
            _videoIndexerClient = videoIndexerClient;
        }
        
        [FunctionName("video-indexer-callback")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            IBinder binder,
            ILogger log)
        {
            var encodedVideoPath = req.Query["encodedPath"];
            var indexerId = req.Query["id"];
            var indexerState = req.Query["state"];

            if (indexerState[0] != "Processed")
            {
                log.LogWarning("Callback from indexer. Id:{Id}. State from indexer:{State}", indexerId, indexerState);
                return new OkResult();
            }

            var videoIndexerResult = await _videoIndexerClient.GetIndexerInsights(indexerId);
            var searchIndexModel = TransformVideoIndexModelToSimplifiedStructure(videoIndexerResult, encodedVideoPath);
            var searchIndexJson = JsonConvert.SerializeObject(searchIndexModel);

            await WriteTransformedIndexerResultIntoBlob(binder, indexerId, searchIndexJson);

            return new OkResult();
        }

        private static async Task WriteTransformedIndexerResultIntoBlob(IBinder binder, StringValues indexerId, string indexerInsights)
        {
            var outputContainer = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerStorageContainerAppSetting);
            using (var writer = await binder.BindAsync<TextWriter>(new BlobAttribute($"{outputContainer}/{indexerId}.json")))
            {
                await writer.WriteAsync(indexerInsights);
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// Flattens and simplifies the insights model.
        /// </summary>
        private static SimplifiedVideoInsights TransformVideoIndexModelToSimplifiedStructure(VideoIndexerResult indexerInsights, string encodedMetadataPath)
        {
            return new SimplifiedVideoInsights()
            {
                Content = string.Join(" ", indexerInsights.Videos.SelectMany(video => (video.Insights.Transcript?.Select(transcript => transcript.Text)) ?? Enumerable.Empty<string>())),
                Persons = indexerInsights.SummarizedInsights.Faces.Select(face => face.Name).ToArray(),
                Organizations = Array.Empty<string>(),
                Locations = indexerInsights.SummarizedInsights.NamedLocations.Select(x => x.Name).ToArray(), 
                KeyPhrases = indexerInsights.SummarizedInsights.Keywords.Select(x => x.Name)
                    .Union(indexerInsights.SummarizedInsights.Labels.Select(x => x.Name))
                    .Union(indexerInsights.SummarizedInsights.Topics.Select(x => x.Name))
                    .Union(indexerInsights.SummarizedInsights.Sentiments.Select(x => x.SentimentKey))
                    .Union(indexerInsights.SummarizedInsights.Emotions.Select(x => x.Type))
                    .ToArray(),
                IndexedVideoId = indexerInsights.Id,
                OriginalVideoEncodedMetadataPath = encodedMetadataPath,
                OriginalVideoName = indexerInsights.Name,
                ThumbnailId = indexerInsights.SummarizedInsights.ThumbnailId
            };
        }
    }
}