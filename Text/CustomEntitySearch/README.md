---
topic: sample
languages:
- csharp
products:
- azure-cognitive-services
name: Custom Entity Search sample skill for cognitive search
description: This custom skill finds user defined entities in given texts.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Text/CustomEntitySearch/azuredeploy.json
---

# CustomEntitySearch

This custom skill finds finds user defined entities in given texts.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FText%2FCustomEntitySearch%2Fazuredeploy.json)

## Requirements

These skills have no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function requires Latin-based text (as seen in the sample document provided). The input field "words" is optional, where a user can add a "words.json" file instead.

## Sample Config File
```json
    ["foo1", "foo2"]
```

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "1",
            "data":
            {
                "text":  "Learn how to leverage Azure Storage in your applications with our quickstarts and tutorials.",
                "words": [
                    "learn",
                    "app"
                ]
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "text":  "Azure Storage includes Azure Blobs (objects), Azure Data Lake Storage Gen2, Azure Files, Azure Queues, and Azure Tables.",
                "words": [
                    "bing"
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
            "recordId": "1",
            "data": {
                "EntitiesFound": ["learn", "app"],
                "Entities": [
                    {
                        "Name": "Learn",
                        "matchIndex": 1
                    },
                    {
                        "Name": "app",
                        "MatchIndex": 45
                    }
                ]
            }
        },
        {
            "recordId": "foo1",
            "data": 
            {
                "EntitiesFound": [],
                "Entities": []
            }
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
    "description": "Our Custom Entity search custom skill",
    "context": "/document/merged_content/*",
    "uri": "[AzureFunctionEndpointUrl]/api/custom-search?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
        {
            "name": "text",
            "source": "/document/content/"
        },
        {
            "name": "words",
            "source": "/document/merged_content/*"
        }
    ],
    "outputs": [
        {
            "name": "EntitiesFound",
            "targetName": "EntitiesFound"
        },
        {
            "name": "Entities",
            "targetName": "Entities"
        }
    ]
}
```
