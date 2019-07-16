// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    public class HocrPage
    {
        readonly StringWriter metadata = new StringWriter();
        readonly StringWriter text = new StringWriter() { NewLine = " " };

        public HocrPage(OcrImageMetadata imageMetadata, int pageNumber, Dictionary<string, string> wordAnnotations = null)
        {
            // page
            metadata.WriteLine($"<div class='ocr_page' id='page_{pageNumber}' title='image \"{imageMetadata.ImageStoreUri}\"; bbox 0 0 {imageMetadata.Width} {imageMetadata.Height}; ppageno {pageNumber}'>");
            metadata.WriteLine($"<div class='ocr_carea' id='block_{pageNumber}_1'>");

            IEnumerable<IEnumerable<NormalizedWord>> wordGroups =
                (imageMetadata.HandwrittenLayoutText != null && imageMetadata.LayoutText != null) ?
                    (imageMetadata.HandwrittenLayoutText.Text.Length > imageMetadata.LayoutText.Text.Length) ?
                        BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words) :
                        BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words) :
                    (imageMetadata.HandwrittenLayoutText != null) ?
                        BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.HandwrittenLayoutText.Lines, imageMetadata.HandwrittenLayoutText.Words) :
                        BuildOrderedWordGroupsFromBoundingBoxes(imageMetadata.LayoutText.Lines, imageMetadata.LayoutText.Words);

            int line = 0;
            int wordIndex = 0;
            foreach (IEnumerable<NormalizedWord> words in wordGroups)
            {
                metadata.WriteLine($"<span class='ocr_line' id='line_{pageNumber}_{line}' title='baseline -0.002 -5; x_size 30; x_descenders 6; x_ascenders 6'>");

                foreach (NormalizedWord word in words)
                {
                    string annotation = "";
                    if (wordAnnotations != null && wordAnnotations.TryGetValue(word.Text, out string wordAnnotation))
                    {
                        annotation = $"data-annotation='{wordAnnotation}'";
                    }
                    string bbox = word.BoundingBox != null && word.BoundingBox.Count == 4 ? $"bbox {word.BoundingBox[0].X} {word.BoundingBox[0].Y} {word.BoundingBox[2].X} {word.BoundingBox[2].Y}" : "";
                    metadata.WriteLine($"<span class='ocrx_word' id='word_{pageNumber}_{line}_{wordIndex}' title='{bbox}' {annotation}>{word.Text}</span>");
                    text.WriteLine(word.Text);
                    wordIndex++;
                }
                line++;
                metadata.WriteLine("</span>"); // Line
            }
            metadata.WriteLine("</div>"); // Reading area
            metadata.WriteLine("</div>"); // Page
        }

        public string Metadata => metadata.ToString();
        public string Text => text.ToString();

        private IEnumerable<IEnumerable<NormalizedWord>> BuildOrderedWordGroupsFromBoundingBoxes(List<NormalizedLine> lines, List<NormalizedWord> words)
        {
            var lineGroups = new List<LineWordGroup>();
            foreach (NormalizedLine line in lines)
            {
                var currGroup = new LineWordGroup(line);
                foreach (NormalizedWord word in words)
                {
                    if (CheckIntersection(line.BoundingBox, word.BoundingBox) && line.Text.Contains(word.Text))
                    {
                        currGroup.Words.Add(word);
                    }
                }
                lineGroups.Add(currGroup);
            }
            return lineGroups
                .OrderBy(grp =>grp.Line.BoundingBox.Select(p => p.Y).Max())
                .Select(grp => grp.Words.FirstOrDefault()?.BoundingBox == null ? grp.Words.ToArray() : grp.Words.OrderBy(l => l.BoundingBox[0].X).ToArray());
        }

        private bool CheckIntersection(List<Point> line, List<Point> word)
        {
            int lineLeft = line.Select(pt => pt.X).Min();
            int lineTop = line.Select(pt => pt.Y).Min();
            int lineRight = line.Select(pt => pt.X).Max();
            int lineBottom = line.Select(pt => pt.Y).Max();

            int wordLeft = word.Select(pt => pt.X).Min();
            int wordTop = word.Select(pt => pt.Y).Min();
            int wordRight = word.Select(pt => pt.X).Max();
            int wordBottom = word.Select(pt => pt.Y).Max();

            return !(wordLeft > lineRight
                || wordRight < lineLeft
                || wordTop > lineBottom
                || wordBottom < lineTop);
        }

        private class LineWordGroup
        {
            public NormalizedLine Line { get; }
            public List<NormalizedWord> Words { get; } = new List<NormalizedWord>();

            public LineWordGroup(NormalizedLine line) => Line = line;
        }
    }
}
