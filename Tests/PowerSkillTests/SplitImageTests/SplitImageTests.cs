// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Tests.SplitImageTests
{
    [TestClass]
    public class SplitImageTests
    {        
        [TestMethod]
        [DataRow(@"SplitImageTests\Assets\bigImage.bmp")]
        [DataRow(@"SplitImageTests\Assets\bigImage.gif")]
        [DataRow(@"SplitImageTests\Assets\bigImage.jpg")]
        [DataRow(@"SplitImageTests\Assets\bigImage.png")]
        [DataRow(@"SplitImageTests\Assets\bigImage.tif")]
        [DataRow(@"SplitImageTests\Assets\bigImage_deflate.tif")]
        public async Task CanSplitImages(string imageLocation)
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), imageLocation);
            await TestVerticalSplit(imageLoc);
        }

        [TestMethod]
        public async Task DoesntSplitSmallImages()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\smallImage.png");
            var splitImages = await Helpers.QuerySkill(
                            PowerSkills.Vision.SplitImage.SplitImage.RunSplitImageSkill,
                            new
                            {
                                imageLocation = imageLoc,
                                sasToken = string.Empty
                            },
                            "splitImages"
                        ) as JArray;

            Assert.AreEqual(1, splitImages.Count);
            Assert.AreEqual(726, splitImages[0]["width"]);
            Assert.AreEqual(296, splitImages[0]["height"]);
        }

        [TestMethod]
        public async Task ImageUriIsRequired()
        {
            WebApiSkillResponse skillOutput = await Helpers.QueryFunction(Helpers.BuildPayload(new { }), PowerSkills.Vision.SplitImage.SplitImage.RunSplitImageSkill);

            Assert.AreEqual(1, skillOutput.Values[0].Errors.Count);
            Assert.AreEqual("Parameter 'imageUrl' is required to be present and a valid uri.", skillOutput.Values[0].Errors[0].Message);
        }

        private static async Task TestVerticalSplit(string imageLoc)
        {
            var splitImages = await Helpers.QuerySkill(
                PowerSkills.Vision.SplitImage.SplitImage.RunSplitImageSkill,
                new
                {
                    imageLocation = imageLoc,
                    sasToken = string.Empty
                },
                "splitImages"
            ) as JArray;

            Assert.AreEqual(7, splitImages.Count);

            foreach (var image in splitImages)
            {
                Assert.AreEqual(1000, image["width"]);

                if (image != splitImages.Last)
                {
                    Assert.AreEqual(4000, image["height"]);
                }
                else
                {
                    // last "page" isn't max size
                    Assert.AreEqual(2000, image["height"]);
                }
            }
        }
    }
}
