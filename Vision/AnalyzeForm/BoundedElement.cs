namespace AzureCognitiveSearch.PowerSkills.Vision.AnalyzeForm
{
    public class BoundedElement
    {
        public string Text { get; set; }
        public double[] BoundingBox { get; set; }

        public double Confidence { get; set; }
    }
}
