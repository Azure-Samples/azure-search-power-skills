// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using AzureCognitiveSearch.PowerSkills.Common;
using System;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Web;
using System.Collections.Specialized;

namespace AzureCognitiveSearch.PowerSkills.Vision.SplitImage
{
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
                    var imageUrl = inRecord.Data["imageLocation"] as string; // no url parameters
                    var sasToken = inRecord.Data["sasToken"] as string; // includes ? to start query params

                    string fullUri = CombineSasTokenWithUri(imageUrl, sasToken);
                    JArray splitImages = new JArray();

                    using (WebClient client = new WebClient())
                    {
                        var fileData = client.DownloadData(new Uri(fullUri));

                        using (var stream = new MemoryStream(fileData))
                        {
                            var originalImage = Image.Load(stream);

                            // chunk the document up into pieces
                            // overlap the chunks to reduce the chances of cutting words in half
                            // (and not being able to OCR that data)
                            // TODO: could probably be smarter about this
                            for (int x = 0; x < originalImage.Width; x += MaxImageDimension)
                            {
                                for (int y = 0; y < originalImage.Height; y += MaxImageDimension)
                                {
                                    int startX = x;
                                    int endX = x + MaxImageDimension >= originalImage.Width
                                                ? originalImage.Width
                                                : x + MaxImageDimension - ImageOverlapInPixels;
                                    int startY = y;
                                    int endY = y + MaxImageDimension >= originalImage.Height
                                                ? originalImage.Height
                                                : y + MaxImageDimension - ImageOverlapInPixels;

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

        public static string CombineSasTokenWithUri(string uri, string sasToken)
        {
            // if this data is combing from blob indexer's metadata_storage_path and metadata_storage_sas_token
            // then we can simply concat them. But lets use uri builder to be safe and support missing characters

            UriBuilder uriBuilder = new UriBuilder(uri);
            NameValueCollection sasParameters = HttpUtility.ParseQueryString(sasToken);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

            foreach(var key in sasParameters.AllKeys)
            {
                // override this url parameter if it already exists
                query[key] = sasParameters[key];
            }

            uriBuilder.Query = query.ToString();
            var finalUrl = uriBuilder.ToString();

            return finalUrl;
        }

        public static byte[] CropImage(
            Image originalImage,
            int startX,
            int endX,
            int startY,
            int endY)
        {
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
