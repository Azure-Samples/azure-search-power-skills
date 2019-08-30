---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
name: Tokenizer sample skill for cognitive search
description: This custom skill extracts normalized non-stop words from a text using the ML.NET library.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Text/Tokenizer/azuredeploy.json
---

# Tokenizer

This custom skill extracts normalized non-stop words from a text using [the ML.NET library](https://docs.microsoft.com/en-us/dotnet/api/microsoft.ml?view=ml-dotnet).

## Requirements

This skills have no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FText%2FTokenizer%2Fazuredeploy.json)

## Settings

The language used for stop word removal can be changed directly in the code.

## tokenizer

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "text": "ML.NET's RemoveDefaultStopWords API removes stop words from tHe text/string. It requires the text/string to be tokenized beforehand."
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
            "recordId": "record1",
            "data": {
                "words": [
                    "mlnets",
                    "removedefaultstopwords",
                    "api",
                    "removes",
                    "stop",
                    "words",
                    "textstring",
                    "requires",
                    "textstring",
                    "tokenized"
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
    "description": "Tokenizer",
    "uri": "[AzureFunctionEndpointUrl]/api/tokenizer?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/content",
    "inputs": [
        {
            "name": "text",
            "source": "/document/content"
        }
    ],
    "outputs": [
        {
            "name": "words",
            "targetName": "words"
        }
    ]
}
```
