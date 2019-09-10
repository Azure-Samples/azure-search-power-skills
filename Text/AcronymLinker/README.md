---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
name: Acronym Linker sample skills for cognitive search
description: These two custom skills (link-acronyms and link-acronyms-list) give definitions for known acronyms.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Text/AcronymLinker/azuredeploy.json
---

# Acronym Linker

These two custom skills (`link-acronyms` and `link-acronyms-list`) give definitions for known acronyms.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FText%2FAcronymLinker%2Fazuredeploy.json)

## Requirements

These skills have no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function uses a JSON file called `acronyms.json` that can be found at the root of this project, and that will be deployed with the function. This file contains a simple dictionary of acronyms to definitions. We provided a sample file with this project that contains definitions for common computer-related acronyms. Please replace this file with your own data, or point `LinkAcronyms` to your data.

## link-acronyms

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "word": "MS"
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "word": "SSL"
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
            "recordId": "foobar2",
            "data": {
                "acronym": {
                    "value": "MS",
                    "description": "Microsoft"
                }
            },
            "errors": [],
            "warnings": []
        },
        {
            "recordId": "foo1",
            "data": {
                "acronym": {
                    "value": "SSL",
                    "description": "Secure Socket Layer"
                }
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```

## link-acronyms-list

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "words": [ "MS",  "SSL" ]
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
            "recordId": "foobar2",
            "data": {
                "acronyms": [
                    {
                        "value": "MS",
                        "description": "Microsoft"
                    },
                    {
                        "value": "SSL",
                        "description": "Secure Socket Layer"
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
    "description": "Acronym linker",
    "uri": "[AzureFunctionEndpointUrl]/api/link-acronyms-list?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/normalized_images/*/layoutText",
    "inputs": [
        {
            "name": "words",
            "source": "/document/normalized_images/*/layoutText/words/*/text"
        }
    ],
    "outputs": [
        {
            "name": "acronyms",
            "targetName": "acronyms"
        }
    ]
}
```
