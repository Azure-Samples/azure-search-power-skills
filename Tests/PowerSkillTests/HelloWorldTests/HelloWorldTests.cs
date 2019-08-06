// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

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
            var greeting = await Helpers.QuerySkill(
                Template.HelloWorld.HelloWorld.RunHelloWorld,
                new { Name = "World" },
                "greeting"
            ) as string;
            Assert.AreEqual("Hello, World", greeting);
        }
    }
}
