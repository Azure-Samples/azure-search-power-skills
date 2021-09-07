---
page_type: sample
languages:
- python
products:
- azure
- azure-search
- azure-cognitive-search
name: Text Quality Watchdog Custom Skill for Cognitive Search
description: This custom skill calls a pretrained language model to determine the quality of text extracted from the document cracking process.
---

# TextQualityWatchdog

This custom skill calls a pretrained language model to determine the quality of text extracted from the document cracking process.
Text that is well-formed will return a "text_quality_warning" value of 0.
Corrupted/garbled text resulting from factors such as low scan quality or OCR errors will be caught, and will return a "text_quality_warning" value of 1.
When included in an index, this value can be used to filter and examine documents that had low quality text extracted during document cracking.

## Requirements

This function has no additional requirements outside the common requirements described in [the root `README.md` file](../../README.md).

## Optional Model Customization

Please note that the provided "watchdog_model.onnx" classifier was trained on a dataset of 18,000 documents, 50% of which were synthetically generated to correct for class imbalance.
Additionally, labels were generated using an "out-of-vocabulary" metric, which means that data that doesn't use common words often correlates with garbled text.

If you wish to train a new model from a custom dataset, which may include labels assigned in a different way,
please refer to this [sample notebook](./Watchdog/Model/TrainWatchdogModel.ipynb).
To facilitate the dataset building process, we have additionally provided several utility functions in the [Util folder](./Watchdog/Util).

## Deployment

To deploy this skill:
1. Clone this repository.
2. Optionally, use the training notebook to train a model on a custom dataset.
3. Open the TextQualityWatchdog folder in VS Code and deploy the function, following [these instructions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs-code?tabs=python).

## Sample Input:

When this skill is integrated into a skillset, the extracted text from each document will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "text": "This is where the extracted document text goes."
            }
        }
    ]
}
```

## Sample Output:

Text that is well-formed will return a "text_quality_warning" value of 0.

Text that is corrupted/garbled text return a "text_quality_warning" value of 1.

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "text_quality_warning": 0
            }
        }
    ]
}
```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Details on how to configure this skill definition (including specific fields that need to be modified in the skillset, indexer, and index) can be found [here](https://docs.microsoft.com/en-us/azure/search/cognitive-search-defining-skillset).
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "watchdog",
    "description": "Determines quality of text extracted from unstructured documents.",
    "context": "/document",
    "uri": "[AzureFunctionEndpointUrl]/api/TextQualityWatchdog?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT1M",
    "batchSize": 1,
    "degreeOfParallelism": null,
    "inputs": [
        {
            "name": "text",
            "source": "/document/content"
        }
    ],
    "outputs": [
        {
            "name": "text_quality_warning",
            "targetName": "text_quality_warning"
        }
    ],
    "httpHeaders": {}
}
```
