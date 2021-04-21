using System;
using System.Net.Http;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels;
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

            return (await response.Content.ReadAsAsync<VideoUploadResult>()).Id;
        }
        
        public async Task<VideoIndexerResult> GetIndexerInsights(string videoId)
        {
            var endpoint = "https://api.videoindexer.ai";
            var accountId = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountIdAppSetting);
            var location = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerLocationAppSetting);
            var videoIndexerAccountKey  = Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerAccountKeyAppSetting);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}/auth/{location}/Accounts/{accountId}/AccessToken?allowEdit=true");
            request.Headers.Add("Ocp-Apim-Subscription-Key", videoIndexerAccountKey);

            var accessTokenResponse = await _httpClient.SendAsync(request);
            accessTokenResponse.EnsureSuccessStatusCode();
            var accessToken = await accessTokenResponse.Content.ReadAsAsync<string>();
            _logger.LogInformation("Retrieved access token for Video Indexer");

            var response = await _httpClient.GetAsync($"{endpoint}/{location}/Accounts/{accountId}/Videos/{videoId}/Index?includeStreamingUrls=True&accessToken={accessToken}");
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Retrieved insights for video Id:{Id}", videoId);

            return await response.Content.ReadAsAsync<VideoIndexerResult>();
        }
    }
}