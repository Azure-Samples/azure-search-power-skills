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
