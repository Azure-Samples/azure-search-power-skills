namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class Image
    {
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public Provider[] Provider { get; set; }
        public string HostPageUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
