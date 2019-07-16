// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;

namespace AzureCognitiveSearch.PowerSkills.Vision.ImageStore
{
    public class Image
    {
        public string Name { get; }
        public string Data { get; }
        public string MimeType { get; }

        public Image(string name, byte[] data, string mimeType) : this(name, Convert.ToBase64String(data), mimeType)
        { }

        public Image(string name, string data, string mimeType)
        {
            Name = name;
            Data = data;
            MimeType = mimeType;
        }
    }
}
