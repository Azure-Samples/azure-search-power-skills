using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class VideoIndexerClient : IVideoIndexerClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<VideoIndexerClient> _logger;

        public VideoIndexerClient(HttpClient client, ILogger<VideoIndexerClient> logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        public async Task<string> SubmitVideoIndexingJob(string videoBlobUrl, string encodedVideoUrl, string videoName)
        {
            var endpoint = "https://api.videoindexer.ai";
            
            var accountId = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountIdAppSetting);
            var location = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerLocationAppSetting);
            var videoIndexerAccountKey  = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountKeyAppSetting);
            var functionCode = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerCallbackFunctionCodeAppSetting);
            var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            var callbackUrl = $"https://{hostName}/api/video-indexer-callback?code={functionCode}&encodedPath={encodedVideoUrl}";
            _logger.LogInformation("Passing callback to indexer at: {CallbackUrl}", callbackUrl.Replace(functionCode, functionCode.Substring(0, 5) + "XXX"));
            var privacy = "Private";

            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", videoIndexerAccountKey);

            var accessToken = JsonConvert.DeserializeObject<string>(await _httpClient.GetStringAsync($"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true"));
            
            _logger.LogInformation("Retrieved access token for Video Indexer");

            videoBlobUrl = Uri.EscapeDataString(videoBlobUrl);

            var response = await _httpClient.PostAsync(
                $"{endpoint}/{location}/Accounts/{accountId}/Videos?accessToken={accessToken}&name={Uri.EscapeDataString(videoName)}&videoUrl={videoBlobUrl}&privacy={privacy}&callbackUrl={Uri.EscapeDataString(callbackUrl)}",
                new MultipartFormDataContent());

            _logger.LogInformation("Submitted video {VideoName} for indexing", videoName);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}