---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-cognitive-services
name: Bing Entity Search sample skill for cognitive search
description: This custom skill finds rich and structured information about public figures, locations, or organizations.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Text/BingEntitySearch/azuredeploy.json
---

# BingEntitySearch

This custom skill finds rich and structured information about public figures, locations, or organizations.

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Bing Entity Search](https://azure.microsoft.com/en-us/services/cognitive-services/bing-entity-search-api/) service.

## Settings

This function requires a `BING_API_KEY` setting set to a valid Azure Bing Entity Search API key.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FText%2FBingEntitySearch%2Fazuredeploy.json)

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "name":  "Pablo Picasso"
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "name":  "Microsoft"
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
            "recordId": "foobar2",
            "data": {
                "name": "Pablo Picasso",
                "description": "Pablo Ruiz Picasso was a Spanish painter, sculptor, [...]",
                "imageUrl": "https://www.bing.com/th?id=AMMS_e8c719d1c081e929c60a2f112d659d96&w=110&h=110&c=12&rs=1&qlt=80&cdv=1&pid=16.2",
                "url": "http://en.wikipedia.org/wiki/Pablo_Picasso",
                "licenseAttribution": "Text under CC-BY-SA license",
                "entities": "{...}"
            }
        },
        "..."
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Our new Bing entity search custom skill",
    "context": "/document/merged_content/organizations/*",
    "uri": "[AzureFunctionEndpointUrl]/api/entity-search?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
        {
            "name": "name",
            "source": "/document/merged_content/organizations/*",
            "sourceContext": null,
            "inputs": []
        }
    ],
    "outputs": [
        {
            "name": "description",
            "targetName": "description"
        }
    ],
    "httpHeaders": {}
}
```
