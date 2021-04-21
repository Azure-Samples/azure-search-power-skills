using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class VideoIndexerBlobClient : IVideoIndexerBlobClient
    {
        public async Task<string> GetSasKey(string videoUrl)
        {
            var client = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable(VideoIndexerAppSettings.MediaIndexerStorageConnectionStringAppSetting));
            var blob = await client.CreateCloudBlobClient().GetBlobReferenceFromServerAsync(new Uri(videoUrl));
            return blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTimeOffset.Now.AddMinutes(10)
            });
        }
    }
}