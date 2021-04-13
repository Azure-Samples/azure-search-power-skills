using System;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AzureCognitiveSearch.PowerSkills.Tests.VideoTests
{
    [TestClass]
    public class VideoIndexerTests
    {
        [TestMethod]
        public async Task VideoIndexerUploadsVideo()
        {
            var sasKey = Guid.NewGuid().ToString();
            var jobId = new Random(Environment.TickCount).Next(1000, 99999).ToString();

            var indexer = new VideoIndexer(new FakeVideoBlobClient(sasKey),
                new FakeVideoIndexerClient(sasKey, jobId));
            var outputJobId = await Helpers.QuerySkill(
                indexer.RunVideoIndexer,
                new
                {
                    metadata_storage_path = "aHR0cHM6Ly9rbXZncmZzdHIuYmxvYi5jb3JlLndpbmRvd3MubmV0L2RvY3VtZW50cy9waXBlbGluZS5tcDQ1",
                    metadata_storage_name = "pipeline.mp4"
                },
                "jobId"
            ) as string;
            Assert.AreEqual(jobId, outputJobId);
        }

        internal class FakeVideoBlobClient : IVideoIndexerBlobClient
        {
            private readonly string _sasKey;

            public FakeVideoBlobClient(string sasKey)
            {
                _sasKey = sasKey;
            }
            public Task<string> GetSasKey(string blobPath)
            {
                return Task.FromResult(_sasKey);
            }
        }

        internal class FakeVideoIndexerClient : IVideoIndexerClient
        {
            private readonly string _sasKey;
            private readonly string _jobId;

            public FakeVideoIndexerClient(string sasKey, string jobId)
            {
                _sasKey = sasKey;
                _jobId = jobId;
            }
            public Task<string> SubmitVideoIndexingJob(string videoBlobUrl, string encodedVideoUrl, string videoName)
            {
                if (videoBlobUrl.Contains(_sasKey))
                {
                    return Task.FromResult(_jobId);
                }

                throw new InvalidOperationException("Unexpected blob Url");
            }

            public Task<VideoIndexerResult> GetIndexerInsights(string videoId)
            {
                throw new NotImplementedException();
            }
        }
    }
}