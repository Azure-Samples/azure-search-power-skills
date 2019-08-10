// Copyright (c) Microsoft. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureCognitiveSearch.PowerSkills.Vision.Hocr
{
    // Uses HOCR format for representing the document metadata.
    // See https://en.wikipedia.org/wiki/HOCR
    public class HocrDocument
    {
        private readonly string header = @"
            <?xml version='1.0' encoding='UTF-8'?>
            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>
            <html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'>
            <head>
                <title></title>
                <meta http-equiv='Content-Type' content='text/html;charset=utf-8' />
                <meta name='ocr-system' content='Microsoft Cognitive Services' />
                <meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par ocr_line ocrx_word'/>
            </head>
            <body>";
        private readonly string footer = "</body></html>";

        public HocrDocument(IEnumerable<HocrPage> pages)
        {
            Metadata = header + Environment.NewLine + string.Join(Environment.NewLine, pages.Select(p => p.Metadata)) + Environment.NewLine + footer;
            Text = string.Join(Environment.NewLine, pages.Select(p => p.Text));
        }

        public string Metadata { get; set; }

        public string Text { get; set; }
    }
}
