using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureCognitiveSearch.PowerSkills.Video.VideoIndexer.Startup))]

namespace AzureCognitiveSearch.PowerSkills.Video.VideoIndexer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient<IVideoIndexerClient, VideoIndexerClient>();
            builder.Services.AddSingleton<IVideoIndexerBlobClient, VideoIndexerBlobClient>();
        }
    }
}