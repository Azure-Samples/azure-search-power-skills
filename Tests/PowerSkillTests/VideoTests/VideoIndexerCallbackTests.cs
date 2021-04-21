using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer;
using AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.VideoIndexerModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace AzureCognitiveSearch.PowerSkills.Tests.VideoTests
{
    [TestClass]
    public class VideoIndexerCallbackTests
    {
        private const string TestVideoId = "12345";
        private const string TestVideoThumbnailId = "sdfklj23r";

        private const string TestEncodedPath =
            "aHR0cHM6Ly9rbXZncmZzdHIuYmxvYi5jb3JlLndpbmRvd3MubmV0L2RvY3VtZW50cy9waXBlbGluZS5tcDQ1";

        [TestMethod]
        public async Task VideoIndexerCallbackUploadsBlob()
        {
            var indexer = new VideoIndexingCallback(new FakeVideoIndexerClient());

            var outputBinder = new AttributeCapturingBinder();
            var response = await indexer.RunAsync(
                new DefaultHttpRequest(new DefaultHttpContext())
                {
                    ContentType = "application/json; charset=utf-8",
                    Method = "GET",
                    QueryString =
                        new QueryString(
                            $"?encodedPath={TestEncodedPath}&id={TestVideoId}&state=Processed")
                },
                outputBinder,
                new LoggerFactory().CreateLogger("local"));

            Assert.IsInstanceOfType(response, typeof(OkResult));
            var outputBlobAttribute = (BlobAttribute) outputBinder.BindingAttribute;
            Assert.AreEqual(outputBlobAttribute.BlobPath, $"/{TestVideoId}.json");

            object expectedBlobContent = new
            {
                content = "This is transcript one This is transcript two",
                keyPhrases = new [] { "Keyword1","Keyword2","Label1","Label2","mining","content","optimistic","Happy" },
                organizations = Array.Empty<string>(),
                persons = new [] { "Graeme Foster" },
                locations = new [] { "Perth" },
                indexedVideoId = TestVideoId,
                thumbnailId = TestVideoThumbnailId,
                originalVideoEncodedMetadataPath = TestEncodedPath,
                originalVideoName = "pipeline.mp4"
            };

            Assert.AreEqual(
                JsonConvert.SerializeObject(expectedBlobContent),
                outputBinder.BindingStringWriter.ToString());
        }

        internal class FakeVideoIndexerClient : IVideoIndexerClient
        {
            public Task<string> SubmitVideoIndexingJob(string videoBlobUrl, string encodedVideoUrl, string videoName)
            {
                throw new NotImplementedException();
            }

            public Task<VideoIndexerResult> GetIndexerInsights(string videoId)
            {
                return Task.FromResult(new VideoIndexerResult()
                {
                    Id = TestVideoId,
                    Name = "pipeline.mp4",
                    Videos = new[]
                    {
                        new Video.VideoIndexer.VideoIndexerModels.Video()
                        {
                            Insights = new Insight()
                            {
                                Transcript = new[]
                                {
                                    new Transcript() {Text = "This is transcript one"},
                                    new Transcript() {Text = "This is transcript two"},
                                }
                            }
                        }
                    },
                    SummarizedInsights = new SummarizedInsights()
                    {
                        Emotions = new[]
                        {
                            new Emotion() {Type = "Happy"}
                        },
                        Faces = new[]
                        {
                            new Face() {Confidence = 0.9, Name = "Graeme Foster", Title = "Mr"}
                        },
                        Keywords = new[]
                        {
                            new Keyword {Name = "Keyword1"},
                            new Keyword {Name = "Keyword2"},
                        },
                        Labels = new[]
                        {
                            new Label {Name = "Label1"},
                            new Label {Name = "Label2"},
                        },
                        Sentiments = new[]
                        {
                            new Sentiment {SentimentKey = "content"},
                            new Sentiment {SentimentKey = "optimistic"},
                        },
                        Topics = new[]
                        {
                            new Topic() {Name = "mining", Confidence = 0.75}
                        },
                        NamedLocations = new[]
                        {
                            new NamedLocation() {Confidence = 0.8, Name = "Perth"}
                        },
                        ThumbnailId = TestVideoThumbnailId
                    }
                });
            }
        }

        internal class AttributeCapturingBinder : IBinder
        {
            public Task<T> BindAsync<T>(Attribute attribute,
                CancellationToken cancellationToken = new CancellationToken())
            {
                BindingAttribute = attribute;
                if (typeof(T) == typeof(TextWriter))
                {
                    BindingStringWriter = new StringWriter();
                    return Task.FromResult((T) (object) BindingStringWriter);
                }

                throw new NotSupportedException();
            }

            public Attribute BindingAttribute { get; private set; }
            public StringWriter BindingStringWriter { get; private set; }
        }
    }
}