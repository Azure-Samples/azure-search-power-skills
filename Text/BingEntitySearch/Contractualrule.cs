namespace AzureCognitiveSearch.PowerSkills.Text.BingEntitySearch
{
    public class Contractualrule
    {
        public string _type { get; set; }
        public string TargetPropertyName { get; set; }
        public bool MustBeCloseToContent { get; set; }
        public License License { get; set; }
        public string LicenseNotice { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
