using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.HelloWorldTests
{

    [TestClass]
    public class HelloWorldTests
    {        
        [TestMethod]
        public async Task HelloWorld()
        {
            WebApiSkillResponse helloWorldOutput = await Helpers.QueryFunction(
                Helpers.BuildPayload(new {Name = "World"}),
                request => Template.HelloWorld.HelloWorld.RunHelloWorld(request, new LoggerFactory().CreateLogger("local"), new Microsoft.Azure.WebJobs.ExecutionContext())
            );
            Assert.AreEqual("Hello, World", helloWorldOutput.Values[0].Data["greeting"]);
        }
    }
}
