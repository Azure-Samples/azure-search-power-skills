using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public interface IVideoIndexerBlobClient
    {
        Task<string> GetSasKey(string blobPath);
    }
}