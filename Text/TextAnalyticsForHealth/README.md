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
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/main/Text/TextAnalyticsForHealth/azuredeploy.json
---

# TextAnalyticsForHealth

This custom skill finds rich and structured information about public figures, locations, or organizations.

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to a [Text Analytics](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/) service.
If deploying through the ARM Template below, a Text Analytics Account will be created for you.

## Settings

This function requires `TEXT_ANALYTICS_API_ENDPOINT` and `TEXT_ANALYTICS_API_KEY` to be set in the settings.This is set automatically when deploying from the ARM Template.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FText%2FTextAnalyticsForHealth%2Fazuredeploy.json)

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "document":  "100mg Ibuprofin"
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "document":  "200mg Tylenol",
                "language":  "en"
            }
        }
    ]
}
```
The skill also accepts options which can be sent as headers in the request.

| Name | Description | Default |
| ---- | ----------- | ------- |
| `timeout` | Allows you to set a timeout (in seconds) which will cause the skill to return early, so it doesn't timeout in the Cognitive Search Pipeline. | `230` |
| `defaultLanguage` | Allows you to set a default language across all documents in case a language is not provided in the input | `"en"` |

## Sample Output:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data": {
                "status": "succeeded",
                "entities": [
                    {...}
                ],
                "relations": [
                    {...}
                ]
            },
            "warnings": [],
            "errors": []
        },
        {...}
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset. **If you deploy using the ARM template you can copy the URI from the output of the template.**
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Our new Bing entity search custom skill",
    "context": "/document",
    "uri": "[AzureFunctionEndpointUrl]/api/entity-search?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "inputs": [
        {
            "name": "document",
            "source": "/document/content",
            "sourceContext": null
        }
    ],
    "outputs": [
        {
            "name": "entities",
            "targetName": "entities"
        },
        {
            "name": "relations",
            "targetName": "relations"
        },
    ],
    "httpHeaders": {}
}
```
