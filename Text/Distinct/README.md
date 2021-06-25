---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
name: Distinct sample skill for cognitive search
description: This custom skill removes duplicates from a list of terms.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/main/Text/Distinct/azuredeploy.json
---

# Distinct

This custom skill removes duplicates from a list of terms.

Terms are considered the same if they only differ by casing, separators such as spaces, or punctuation, or if they have a common entry in the thesaurus.

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FText%2FDistinct%2Fazuredeploy.json)

## Requirements

This skill has no additional requirements than the ones described in [the root `README.md` file](../../README.md).

## Settings

This function uses a JSON file called [`thesaurus.json`](./thesaurus.json) that can be found at the root of this project, and that will be deployed with the function. This file contains a simple list of lists of synonyms. For each list of synonyms, the first is considered the canonical form. Please replace this file with your own data.

## link-acronyms

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "foobar2",
            "data":
            {
                "words": [
                    "MSFT",
                    "U.S.A",
                    "word",
                    "United states",
                    "WOrD",
                    "Microsoft Corp."
                ]
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
                "distinct": {
                    "value": [
                        "Microsoft",
                        "USA",
                        "word"
                    ]
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
    "description": "Distinct entities",
    "uri": "[AzureFunctionEndpointUrl]/api/link-acronyms-list?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/merged_content",
    "inputs": [
        {
            "name": "words",
            "source": "/document/merged_content/organizations"
        }
    ],
    "outputs": [
        {
            "name": "distinct",
            "targetName": "distinct_organizations"
        }
    ]
}
```
