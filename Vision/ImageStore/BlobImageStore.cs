// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.PowerSkills.Vision.ImageStore
{
    public class BlobImageStore
    {
        private readonly CloudBlobContainer libraryContainer;

        public BlobImageStore(string blobConnectionString, string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            libraryContainer = blobClient.GetContainerReference(containerName);
        }

        public async Task<string> UploadImageToLibraryAsync(Stream stream, string name, string mimeType, bool overwrite = false)
        {
            CloudBlockBlob blockBlob = libraryContainer.GetBlockBlobReference(name);
            if (!await blockBlob.ExistsAsync())
            {
                await blockBlob.UploadFromStreamAsync(stream);

                blockBlob.Properties.ContentType = mimeType;
                await blockBlob.SetPropertiesAsync();
            }

            return blockBlob.Uri.ToString();
        }

        public async Task<string> UploadToBlobAsync(byte[] data, string name, string mimeType, bool overwrite = false)
            => await UploadImageToLibraryAsync(new MemoryStream(data), name, mimeType, overwrite);

        public async Task<string> UploadToBlobAsync(Image image, bool overwrite = false)
            => await UploadToBlobAsync(image.Data, image.Name, image.MimeType, overwrite);

        public async Task<string> UploadToBlobAsync(string data, string name, string mimeType, bool overwrite = false)
            => await UploadToBlobAsync(Convert.FromBase64String(data), name, mimeType, overwrite);

        public async Task<Image> DownloadFromBlobAsync(string imageUrl)
        {
            string imageName = new Uri(imageUrl).Segments.Last();
            CloudBlockBlob blockBlob = libraryContainer.GetBlockBlobReference(imageName);
            if (await blockBlob.ExistsAsync())
            {
                using (var stream = new MemoryStream())
                {
                    string mimeType = blockBlob.Properties.ContentType;
                    await blockBlob.DownloadToStreamAsync(stream);
                    byte[] data = stream.ToArray();
                    return new Image(imageName, data, mimeType);
                }
            }
            return null;
        }
    }
}
