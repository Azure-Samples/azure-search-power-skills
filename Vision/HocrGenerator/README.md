---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
urlFragment: azure-hocr-generator-sample
name: hOCR generator sample skill for cognitive search
description: This custom skill generates an hOCR document from the output of the OCR skill.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Vision/HocrGenerator/azuredeploy.json
---

# hOCR Generator

This custom skill generates an [hOCR](https://en.wikipedia.org/wiki/HOCR) document from the output of [the OCR skill](https://docs.microsoft.com/azure/search/cognitive-search-skill-ocr).

## Requirements

This skill has no additional requirements than the ones described in [the root `README.md` file](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/README.md).

## Settings

This function doesn't require any application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FVision%2FImageStore%2Fazuredeploy.json)

## Sample Input:

```json
{
	"values": [
	    {
	        "recordId": "r1",
	        "data": {
	            "ocrImageMetadataList": [
	                {
	                    "layoutText": {
	                        "language": "en",
	                        "text": "Hello World. -John",
	                        "lines": [
	                            {
	                                "boundingBox": [
	                                    { "x": 10, "y": 10 },
	                                    { "x": 50, "y": 10 },
	                                    { "x": 50, "y": 30 },
	                                    { "x": 10, "y": 30 }
	                                ],
	                                "text": "Hello World."
	                            },
	                            {
	                                "boundingBox": [
	                                    { "x": 110, "y": 10 },
	                                    { "x": 150, "y": 10 },
	                                    { "x": 150, "y": 30 },
	                                    { "x": 110, "y": 30 }
	                                ],
	                                "text": "-John"
	                            }
	                        ],
	                        "words": [
	                            {
	                                "boundingBox": [
	                                    { "x": 10, "y": 10 },
	                                    { "x": 50, "y": 10 },
	                                    { "x": 50, "y": 14 },
	                                    { "x": 10, "y": 14 }
	                                ],
	                                "text": "Hello"
	                            },
	                            {
	                                "boundingBox": [
	                                    { "x": 10, "y": 16 },
	                                    { "x": 50, "y": 16 },
	                                    { "x": 50, "y": 30 },
	                                    { "x": 10, "y": 30 }
	                                ],
	                                "text": "World."
	                            },
	                            {
	                                "boundingBox": [
	                                    { "x": 110, "y": 10 },
	                                    { "x": 150, "y": 10 },
	                                    { "x": 150, "y": 30 },
	                                    { "x": 110, "y": 30 }
	                                ],
	                                "text": "-John"
	                            }
	                        ]
	                    },
	                    "imageStoreUri": "https://[somestorageaccount].blob.core.windows.net/pics/lipsum.tiff",
	                    "width": 40,
	                    "height": 200
	                }
	            ],
	            "wordAnnotations": [
	                {
	                    "value": "Hello",
	                    "description": "An annotation on 'Hello'"
	                }
	            ]
	        }
	    }
	]
}
```

## Sample Output:

```json
{
    "values": [
        {
            "recordId": "r1",
            "data": {
                "hocrDocument": {
                    "metadata": "\r\n            <?xml version='1.0' encoding='UTF-8'?>\r\n            <!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>\r\n            <html xmlns='http://www.w3.org/1999/xhtml' xml:lang='en' lang='en'>\r\n            <head>\r\n                <title></title>\r\n                <meta http-equiv='Content-Type' content='text/html;charset=utf-8' />\r\n                <meta name='ocr-system' content='Microsoft Cognitive Services' />\r\n                <meta name='ocr-capabilities' content='ocr_page ocr_carea ocr_par ocr_line ocrx_word'/>\r\n            </head>\r\n            <body>\r\n<div class='ocr_page' id='page_0' title='image \"https://[somestorageaccount].blob.core.windows.net/pics/lipsum.tiff\"; bbox 0 0 40 200; ppageno 0'>\r\n<div class='ocr_carea' id='block_0_1'>\r\n<span class='ocr_line' id='line_0_0' title='baseline -0.002 -5; x_size 30; x_descenders 6; x_ascenders 6'>\r\n<span class='ocrx_word' id='word_0_0_0' title='bbox 10 10 50 14' data-annotation='An annotation on 'Hello''>Hello</span>\r\n<span class='ocrx_word' id='word_0_0_1' title='bbox 10 16 50 30' >World.</span>\r\n</span>\r\n<span class='ocr_line' id='line_0_1' title='baseline -0.002 -5; x_size 30; x_descenders 6; x_ascenders 6'>\r\n<span class='ocrx_word' id='word_0_1_2' title='bbox 110 10 150 30' >-John</span>\r\n</span>\r\n</div>\r\n</div>\r\n\r\n</body></html>",
                    "text": "Hello World. -John "
                }
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Generate HOCR for webpage rendering",
    "uri": "[AzureFunctionEndpointUrl]/api/hocr-generator?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document",
    "inputs": [
        {
            "name": "ocrImageMetadataList",
            "source": "/document/normalized_images/*/ocrImageMetadata"
        },
        {
            "name": "wordAnnotations",
            "source": "/document/acronyms"
        }
    ],
    "outputs": [
        {
            "name": "hocrDocument",
            "targetName": "hocrDocument"
        }
    ]
}
```
