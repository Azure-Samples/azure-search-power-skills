using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public interface IVideoIndexerClient
    {
        Task<string> SubmitVideoIndexingJob(string videoBlobUrl, string encodedVideoUrl, string videoName);
    }
}