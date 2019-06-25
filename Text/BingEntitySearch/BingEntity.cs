namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class BingEntity
    {

        public Contractualrule[] contractualRules { get; set; }
        public Image Image { get; set; }
        public string Description { get; set; }
        public string BingId { get; set; }
        public string WebSearchUrl { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public EntityPresentationInfo EntityPresentationInfo { get; set; }
    }
}
