// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using AzureCognitiveSearch.PowerSkills.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Vision.SplitImage
{
    /// <summary>
    /// DISCLAIMER: This skill uses third third-party packages. Use at your own risk. 
    /// 
    //    TiffLibrary.ImageSharpAdapter
    //        https://www.nuget.org/packages/SixLabors.ImageSharp/1.0.0-beta0007
    //        https://github.com/SixLabors/ImageSharp/
    ///
    //    TiffLibrary.ImageSharpAdapter
    //        https://www.nuget.org/packages/TiffLibrary.ImageSharpAdapter/0.5.134-beta
    //        https://github.com/yigolden/TiffLibrary
    ///
    //    If you need to verify the source code of either package, you can find its source code at corresponding link and verify the contents of the package corresponds to the source in that repository using publicly available tools.
    /// 
    /// Splits a large image into smaller, overlapping chunks to allow their use in other vision skills such as OCR
    /// Supported file types: .bmp, .gif, .jpg, .tif, .png
    /// </summary>
    public static class SplitImage
    {
        private static int MaxImageDimension = 4000; // maximum size of image in cognitive service pipeline
        private static int ImageOverlapInPixels = 100;

        static SplitImage()
        {
            SixLabors.ImageSharp.Configuration.Default.Configure(new TiffLibrary.ImageSharpAdapter.TiffConfigurationModule());
        }

        [FunctionName("split-image")]
        public static async Task<IActionResult> RunSplitImageSkill(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext executionContext)
        {
            log.LogInformation("Split Image Custom Skill: C# HTTP trigger function processed a request.");

            string skillName = executionContext.FunctionName;
            IEnumerable<WebApiRequestRecord> requestRecords = WebApiSkillHelpers.GetRequestRecords(req);
            if (requestRecords == null)
            {
                return new BadRequestObjectResult($"{skillName} - Invalid request record array.");
            }

            WebApiSkillResponse response = WebApiSkillHelpers.ProcessRequestRecords(skillName, requestRecords,
                (inRecord, outRecord) => {
                    var imageUrl = (inRecord.Data.TryGetValue("imageLocation", out object imageUrlObject) ? imageUrlObject : null) as string;
                    var sasToken = (inRecord.Data.TryGetValue("imageLocation", out object sasTokenObject) ? sasTokenObject : null) as string;

                    if (string.IsNullOrWhiteSpace(imageUrl))
                    {
                        outRecord.Errors.Add(new WebApiErrorWarningContract() { Message = $"Parameter '{nameof(imageUrl)}' is required to be present and a valid uri." });
                        return outRecord;
                    }

                    JArray splitImages = new JArray();

                    using (WebClient client = new WebClient())
                    {
                        byte[] fileData = executionContext.FunctionName == "unitTestFunction" 
                                            ? fileData = File.ReadAllBytes(imageUrl) // this is a unit test, find the file locally
                                            : fileData = client.DownloadData(new Uri(WebApiSkillHelpers.CombineSasTokenWithUri(imageUrl, sasToken))); // download the file from remote server

                        using (var stream = new MemoryStream(fileData))
                        {
                            var originalImage = Image.Load(stream);

                            // chunk the document up into pieces
                            // overlap the chunks to reduce the chances of cutting words in half
                            // (and not being able to OCR that data)
                            for (int x = 0; x < originalImage.Width; x += (MaxImageDimension - ImageOverlapInPixels))
                            {
                                for (int y = 0; y < originalImage.Height; y += (MaxImageDimension - ImageOverlapInPixels))
                                {
                                    int startX = x;
                                    int endX = x + MaxImageDimension >= originalImage.Width
                                                ? originalImage.Width
                                                : x + MaxImageDimension;
                                    int startY = y;
                                    int endY = y + MaxImageDimension >= originalImage.Height
                                                ? originalImage.Height
                                                : y + MaxImageDimension;

                                    var newImageData = CropImage(originalImage, startX, endX, startY, endY);

                                    var imageData = new JObject();
                                    imageData["$type"] = "file";
                                    imageData["data"] = System.Convert.ToBase64String(newImageData);
                                    imageData["width"] = endX - startX;
                                    imageData["height"] = endY - startY;
                                    splitImages.Add(imageData);
                                }
                            }
                        }
                    }

                    outRecord.Data["splitImages"] = splitImages;
                    return outRecord;
                });

            return new OkObjectResult(response);
        }

        public static byte[] CropImage(
            Image originalImage,
            int startX,
            int endX,
            int startY,
            int endY)
        {
            // NOTE: we're not using System.Drawing because its not supported by the Azure Functions platform
            //
            // System.Drawing relies heavily on GDI/GDI+ to do its thing. Because of the somewhat risky nature of those APIs 
            // (large attack surface) they are restricted in the App Service sandbox.
            try
            {
                int newWidth = endX - startX;
                int newHeight = endY - startY;

                using (var outStream = new MemoryStream())
                {
                    var clone = originalImage.Clone(
                                    i => i.Crop(new Rectangle(startX, startY, newWidth, newHeight)));

                    clone.Save(outStream, new JpegEncoder());

                    return outStream.GetBuffer();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to crop image: {e.Message}");
            }
        }
    }
}
