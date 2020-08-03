---
topic: sample
languages:
- csharp
products:
- azure-cognitive-services
name: Get File Extension sample skill for cognitive search
description: This custom skill returns the document's extension and file name without extension.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Utils/GetFileExtension/azuredeploy.json
---

# GetFileExtension

This custom skill returns the document's file extension and the file name without extension to be indexed accordingly.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FUtils%2FGetFileExtension%2Fazuredeploy.json)


## Requirements

These skills have no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "1",
            "data":
            {
                "documentName":  "2020_quarterly_earnings.docx",
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "documentName":  "IMPORTANT_COMPANY_ANNOUNCEMENT.eml",
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
                "extensions" : ".docx",
                "fileName" : "2020_quarterly_earnings"
            },
            "errors": [],
            "warnings": []
        },
        {
            "recordId": "foo1",
            "data": {
                "extensions" : ".eml",
                "fileName" : "IMPORTANT_COMPANY_ANNOUNCEMENT"
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
    "description": "Our Custom Get File Extension custom skill",
    "context": "/document",
    "uri": "[AzureFunctionEndpointUrl]/api/get-file-extension?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
        {
            "name": "documentName",
            "source": "/document/metadata_storage_name/"
        }
    ],
    "outputs": [
        {
            "name": "extension",
            "targetName": "extension"
        },
        {
            "name": "fileName",
            "targetName": "fileName"
        }
    ]
}
```
