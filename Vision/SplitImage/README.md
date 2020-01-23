---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Vision/SplitImage/azuredeploy.json
name: "Split Image sample skill for Azure Cognitive Search"
description: "This Split Image skill can be used on images that are too long to process in the vanilla pipeline. This skill will break a large image up into several images small enough to be processed by OCR."
---

# Split Image
DISCLAIMER: This skill uses third third-party packages. Use at your own risk. 

TiffLibrary.ImageSharpAdapter
    https://www.nuget.org/packages/SixLabors.ImageSharp/1.0.0-beta0007
    https://github.com/SixLabors/ImageSharp/

TiffLibrary.ImageSharpAdapter
    https://www.nuget.org/packages/TiffLibrary.ImageSharpAdapter/0.5.134-beta
    https://github.com/yigolden/TiffLibrary

If you need to verify the source code of either package, you can find its source code at corresponding link and verify the contents of the package corresponds to the source in that repository using publicly available tools.


This Split Image skill can be used on images that are too large to process in the vanilla pipeline. This skill will break a large image up into several images small enough to be processed by OCR.

## Requirements

This skill has no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function doesn't require any application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FVision%2FSplitImage%2Fazuredeploy.json)

## split-image

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "r1",
            "data":
            {
                "imageLocation": "http://blobStorage.com/mypicture",
                "sasToken": "?sas=123&otherSasInfo=456"
            }
        }
    ]
}
```

### Sample Output:

```json
{
    "values": [
        {
            "recordId": "r1",
            "data": {
                "splitImages": [
                    {
                        "$type": "file",
                        "data": "{BASE64 encoded string of first image fragment data}",
                        "width": "1234",
                        "height" : "1235"
                    },
                    {
                        "$type": "file",
                        "data": "{BASE64 encoded string of second image fragment data}",
                        "width": "1234",
                        "height" : "1235"
                    },
                    {
                        "$type": "file",
                        "data": "{BASE64 encoded string of third image fragment data}",
                        "width": "1234",
                        "height" : "1235"
                    }
                ]
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
    "description": "Split large image",
    "uri": "[AzureFunctionEndpointUrl]/api/split-image?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document",
    "inputs": [
        {
            "name": "imageLocation",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "sasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "splitImages",
            "targetName": "splitImages"
        }
    ]
}
```

BONUS examples:
How to OCR these images after they've been split, and then merge the data back into a single field:
```json
    {
        "@odata.type": "#Microsoft.Skills.Custom.OcrSkill",
        "context": "/document/splitImages/*",
        "defaultLanguageCode": null,
        "detectOrientation": true,
        "inputs": [
            {
                "name": "image",
                "source": "/document/splitImages/*"
            }
        ],
        "outputs": [
            {
                "name": "text",
                "targetName": "text"
            },
            {
                "name": "layoutText",
                "targetName": "layoutText"
            }
        ]
    },
    {
        "@odata.type": "#Microsoft.Skills.Custom.MergeSkill",
        "description": "Create merged_text, which includes all the textual representation of each image inserted at the right location in the content field.",
        "context": "/document",
        "insertPreTag": " ",
        "insertPostTag": " ",
        "inputs": [
            {
                "name": "itemsToInsert", "source": "/document/splitImages/*/text"
            }
        ],
        "outputs": [
            {
                "name": "mergedText", "targetName" : "merged_text"
            }
        ]
    }
```