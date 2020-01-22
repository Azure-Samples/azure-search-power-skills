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
        public async Task SplitBitmap()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage.bmp");
            await TestVerticleSplit(imageLoc);
        }

        [TestMethod]
        public async Task SplitSingleLayerGif()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage.gif");
            await TestVerticleSplit(imageLoc);
        }

        [TestMethod]
        public async Task SplitJpeg()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage.jpg");
            await TestVerticleSplit(imageLoc);
        }

        [TestMethod]
        public async Task SplitPng()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage.png");
            await TestVerticleSplit(imageLoc);
        }

        [TestMethod]
        public async Task SplitTiff()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage.tif");
            await TestVerticleSplit(imageLoc);
        }

        [TestMethod]
        public async Task SplitCompressedTiff()
        {
            var imageLoc = Path.Combine(Directory.GetCurrentDirectory(), @"SplitImageTests\Assets\bigImage_deflate.tif");
            await TestVerticleSplit(imageLoc);
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

        private static async Task TestVerticleSplit(string imageLoc)
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
                Assert.AreEqual(2772, image["width"]);

                if (image != splitImages.Last)
                {
                    Assert.AreEqual(4000, image["height"]);
                }
                else
                {
                    // last "page" isn't max size
                    Assert.AreEqual(3000, image["height"]);
                }
            }
        }
    }
}
