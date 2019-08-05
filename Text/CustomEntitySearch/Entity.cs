namespace AzureCognitiveSearch.PowerSkills.Text.CustomEntitySearch
{
    public class Entity
    {
        public string Category { get; set; }
        public string Value { get; set; }
        public int Offset { get; set; }
        public double Confidence { get; set; }
    }
}
