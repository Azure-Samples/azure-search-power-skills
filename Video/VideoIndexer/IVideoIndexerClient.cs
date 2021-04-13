using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public interface IVideoIndexerClient
    {
        Task<string> SubmitVideoIndexingJob(string videoBlobUrl, string encodedVideoUrl, string videoName);
        Task<VideoIndexerResult> GetIndexerInsights(string videoId);
    }
}