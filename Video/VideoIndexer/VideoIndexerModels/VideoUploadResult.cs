using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels
{
    public class VideoUploadResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}