---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-cognitive-services
name: Text Analytics for Health Custom Skill for Cognitive Search
description: This custom skill utilizes the Text Analytics for Health API to identify healthcare entities and relations.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/main/Text/TextAnalyticsForHealth/azuredeploy.json
---

# TextAnalyticsForHealth

This custom skill utilizes the Text Analytics for Health API to identify healthcare entities and relations. You can learn more about how the Text Analytics for Health API works by reading their [docs](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/).

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
                "text": "100mg Ibuprofen"
            }
        },
        {
            "recordId": "foo1",
            "data":
            {
                "text": "200mg Tylenol",
                "languageCode": "en"
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
                "status": "succeeded",
                "entities": [
                    {
                        "text": "100mg",
                        "category": "Dosage",
                        "subCategory": null,
                        "confidenceScore": 1.0,
                        "offset": 0,
                        "length": 5,
                        "dataSources": [],
                        "assertion": null,
                        "normalizedText": null
                    },
                    {
                        "text": "Ibuprofen",
                        "category": "MedicationName",
                        "subCategory": null,
                        "confidenceScore": 1.0,
                        "offset": 6,
                        "length": 9,
                        "dataSources": [],
                        "assertion": null,
                        "normalizedText": null
                    }
                ],
                "relations": [
                    {
                        "relationType": {},
                        "roles": [
                            {
                                "entity": {
                                    "text": "100mg",
                                    "category": "Dosage",
                                    "subCategory": null,
                                    "confidenceScore": 1.0,
                                    "offset": 0,
                                    "length": 5,
                                    "dataSources": [],
                                    "assertion": null,
                                    "normalizedText": null
                                },
                                "name": "Dosage"
                            },
                            {
                                "entity": {
                                    "text": "Ibuprofen",
                                    "category": "MedicationName",
                                    "subCategory": null,
                                    "confidenceScore": 1.0,
                                    "offset": 6,
                                    "length": 9,
                                    "dataSources": [],
                                    "assertion": null,
                                    "normalizedText": null
                                },
                                "name": "Medication"
                            }
                        ]
                    }
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
The skill also accepts options which can be sent as headers in the request.

| Header Name | Description | Default |
| ----------- | ----------- | ------- |
| `timeout` | Allows you to set a timeout (in seconds) which will cause the skill to return early, so it doesn't timeout in the Cognitive Search Pipeline. | `230` |
| `defaultLanguageCode` | Allows you to set a default language across all documents in case a language is not provided in the input | `en` |

**It is important to note** the difference between the timeout option in the header, and the timeout option/behavior when defining a skillset. If a request to a web skill takes longer than the timeout defined in the skillset, it will simply drop the request and move on. With the timeout header option, it allows users to define how long the function should wait before it returns the results it has so far, and returns errors for the documents it has not yet gotten to or not yet received results on.

Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Our new Text Analytics For Health Custom Skill",
    "context": "/document",
    "uri": "[AzureFunctionEndpointUrl]/api/TextAnalyticsForHealth?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "timeout" : "PT3M50S",
    "inputs": [
        {
            "name": "text",
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
    "httpHeaders": {
        "timeout" : "220",
        "defaultLanguageCode" : "en"
    }
}
```
