---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-cognitive-services
name: Custom Vision integration sample skill for cognitive search
urlFragment: azure-customvision-sample
description: This custom skill extracts tags from a trained Custom Vision model (classification or object detection).
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Vision/CustomVision/azuredeploy.json
---

# Custom Vision

This custom skill extracts tags from a trained [Custom Vision](https://www.customvision.ai/) model (classification or object detection).

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to a [Custom Vision](https://www.customvision.ai/) resource. 

You will need to [train a model with your images](https://docs.microsoft.com/en-us/azure/cognitive-services/Custom-Vision-Service/getting-started-build-a-classifier) before you can use this skill. Both classification and object detection models will work.

## Settings

This function requires a `CUSTOM_VISION_PREDICTION_URL` and a `CUSTOM_VISION_API_KEY` settings set to a valid Custom Vision API key and to your Custom Vision prediction endpoint.

The function will attempt to send a binary representation of the input image to Custom Vision, so you should use the `/image` endpoint URL for Custom Vision as [described here](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/use-prediction-api).
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

Optionally, you can set `MAX_PAGES` to control how many pages in the document will be sent to Custom Vision (default is 1, so only the first page will be sent).

Also, you can set `MIN_PROBABILITY_THRESHOLD` which will only return tags with a probability above the desired threshold (default is 0.5).

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FVision%2FCustomVision%2Fazuredeploy.json)

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "pages":  [
                    "Base64 encoding of first page image",
                    "Base64 encoding of second page image"
                    ...
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
            "recordId": "record1",
            "data": {
                "tags" : ["tag 1", "tag 2", ...]
            },
            "errors": null,
            "warnings": null
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
    "context": "/document",
    "uri": "[AzureFunctionEndpointUrl]/api/custom-vision?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
    {
        "name": "pages",
        "source": "/document/normalized_images/*/data"
    }
    ],
    "outputs": [
    {
        "name": "tags",
        "targetName": "tags"
    }
    ]
}

```

## Generating normalized images for the skill to use

The skill requires an array of images - one for each page in the original document - in the `pages` input. 
This example uses the built-in [document cracking pipeline](https://docs.microsoft.com/en-us/azure/search/cognitive-search-concept-image-scenarios#get-normalized-images) to extract normalized images, one for each page in the document. 

Below is an example of the indexer configuration for this step. Notice the use of the `generateNormalizedImagePerPage` image action.

```json
    "parameters": {
        "configuration": {
            "dataToExtract": "contentAndMetadata",
            "imageAction": "generateNormalizedImagePerPage",
            "normalizedImageMaxWidth": 3000,
            "normalizedImageMaxHeight": 3000
        }
    }
```

As an alternative, you can use the built-in [Document Extraction cognitive skill](https://docs.microsoft.com/en-us/azure/search/cognitive-search-skill-document-extraction) as part of your skillset.
