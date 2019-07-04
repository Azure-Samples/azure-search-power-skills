using System.Collections.Generic;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public class OcrLayoutText
    {
        public string Language { get; set; }
        public string Text { get; set; }
        public List<NormalizedLine> Lines { get; set; }
        public List<NormalizedWord> Words { get; set; }
    }
}
