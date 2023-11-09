---
page_type: sample
languages:
- python
products:
- azure
- azure-cognitive-search
name: Custom embedding skill for Azure AI Search
description: The custom skill generates vector embeddings for provided content with the [HuggingFace all-MiniLM-L6-v2 model](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2).
---

# HuggingFace Embeddings Generator

This custom skill enables generation of vector embeddings for text content which might be created/ingested as part of the Azure Cognitive Search pipeline, utilizing the [HuggingFace all-MiniLM-L6-v2 model](https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2).

If you need your data to be chunked before being embedded by this custom skill, consider using the built in [SplitSkill](https://learn.microsoft.com/azure/search/cognitive-search-skill-textsplit). If you are interested in generating embeddings using the [Azure OpenAI service](https://learn.microsoft.com/azure/cognitive-services/openai/), please see the built in [AzureOpenAIEmbeddingSkill](https://learn.microsoft.com/azure/search/cognitive-search-skill-azure-openai-embedding).

## Testing the functionality locally

The code in this skill can be tested locally before deploying to an Azure function. Setup the required parameters inside a `local.settings.json` (to be added, sample below) and follow the instructions in the [Azure functions guide](https://learn.microsoft.com/azure/azure-functions/functions-develop-local) to test this capability locally.

The packages/references required for the code to be functional if running locally are listed in `requirements.txt` in this directory. Be sure that you are using Python 3.9 as your runtime stack.

### Sample local.settings.json

Add a new file named `local.settings.json` inside this skill's working directory with the following contents:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsFeatureFlags": "EnableWorkerIndexing"
  }
}
```

# Deploying the code as an Azure function

This code can be manually deployed to an Azure function app. Clone the repo locally and follow the [Azure functions guide to deploy the function](https://learn.microsoft.com/azure/azure-functions/functions-develop-vs-code?tabs=python). Use Python 3.9 when selecting the runtime stack for the app.

## embed

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "1234",
            "data": {
                "text": "This is a test document."
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
            "recordId": "1234",
            "data": {
                "embedding": [
                    -0.03833850100636482,
                    0.1234646588563919,
                    -0.028642958030104637,
                    . . . 
                ]
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
    "description": "Custom embedding generator",
    "uri": "[AzureFunctionEndpointUrl]/api/embed?code=[AzureFunctionDefaultHostKey]",
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
            "name": "embedding",
            "targetName": "embedding"
        }
    ]
}
```

