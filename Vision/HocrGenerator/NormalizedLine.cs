using System.Collections.Generic;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public class NormalizedLine
    {
        public List<Point> BoundingBox { get; set; }

        public string Text { get; set; }
    }
}
