---
page_type: sample
languages:
- python
products:
- azure
- azure-cognitive-search
- azure-openai
name: Text chunker and embedding skill for Azure Cognitive Search
description: The custom skill chunks content using the open source LangChain text chunker and then utilizes the Azure OpenAI service (https://learn.microsoft.com/azure/cognitive-services/openai/how-to/embeddings?tabs=console) to generate vector embeddings for that content.
---

# Azure OpenAI Embeddings Generator

This custom skill enables generation of vector embeddings for text content which might be created/ingested as part of the Azure Cognitive Search pipeline. This skill requires users to have an [Azure OpenAI service](https://learn.microsoft.com/azure/cognitive-services/openai/) provisioned and uses the specified embedding model of Azure OpenAI to generate the vector embeddings for the content. Due to token length restrictions on these models, the skill offers a text chunker, which is built on top of [LangChain's text splitter](https://api.python.langchain.com/en/latest/api_reference.html#module-langchain.text_splitter).

Details about the various available embeddings model can be found on the [OpenAI concepts page](https://learn.microsoft.com/azure/cognitive-services/openai/concepts/models#embeddings-models). For a versatile model that can work with most search and retrieval applications, it is recommended to utilize the `ada-002` model.

# Requirements

The packages/references required for the code to be functional (if running locally) are listed in `requirements.txt` in this directory. 

In addition to this, the following environment variables ("Application settings" when hosted as an Azure function) are required to be set:

1. `AZURE_OPENAI_API_KEY`: This requires creating an Azure OpenAI resource and a model to be deployed. Follow instructions in the [Azure OpenAI tutorial](https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource?pivots=web-portal) for more information on how to create the resource.

2. `AZURE_OPENAI_API_VERSION`: The API version to use when calling the Azure OpenAI service. More details are in the [reference](https://learn.microsoft.com/azure/cognitive-services/openai/reference#embeddings) page.

3. `AZURE_OPENAI_SERVICE_NAME`: The name of the Azure OpenAI service that was created.

4. `AZURE_OPENAI_EMBEDDING_DEPLOYMENT`: The deployed model to use for generating the embeddings.

## Optional Chunking parameters

The text chunker utilized in this skill exposes a few different parameters, which can be optionally set via inputs from Azure Cognitive Search's skillset execution pipeline. The code currently sets some primitive defaults (which can be changed via environment settings). These parameters are:

1. `num_tokens`: The number of tokens that each chunk should have. Different embedding models have different context token length restrictions, and the code in this repository sets this to 2048. This number can be modified based on the type of content being chunked and the kinds of recall performance required for retrieval scenarios.

2. `min_chunk_size`: The minimum size each chunk needs to be - this can be tweaked to exclude small chunks.

3. `token_overlap`: How many tokens can overlap between subsequent chunks - this can have reasonable impact on relevance of results in retrieval scenarios.

## Optional Embedding parameters

1. `sleep_interval_seconds`: How many seconds to wait between successive attempts to generate embeddings. This can be configured if the existing (rudimentary) retry mechanism doesn't work around [Azure OpenAI rate limits](https://learn.microsoft.com/azure/cognitive-services/openai/quotas-limits).

## Testing the functionality locally

The code in this skill can be tested locally before deploying to an Azure function to play around with different parameters. Setup the required parameters inside a `local.settings.json` (to be added, sample below) and follow the instructions in the [Azure functions guide](https://learn.microsoft.com/azure/azure-functions/functions-develop-local) to test this capability locally.

### Sample local.settings.json

Add a new file named `local.settings.json` inside this skill's working directory with the following contents:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "python",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsFeatureFlags": "EnableWorkerIndexing",
    "AZURE_OPENAI_API_KEY": "<YOUR AZURE OPENAI API KEY>",
    "AZURE_OPENAI_API_VERSION": "<YOUR AZURE OPENAI API VERSION>",
    "AZURE_OPENAI_EMBEDDING_DEPLOYMENT": "<YOUR AZURE OPENAI EMBEDDING MODEL DEPLOYMENT>",
    "AZURE_OPENAI_SERVICE_NAME": "<YOUR AZURE OPENAI SERVICE NAME>",
    "AZURE_OPENAI_EMBEDDING_SLEEP_INTERVAL_SECONDS": "<Interval in seconds between embedding api calls>"
  }
}
```

# Deploying the code as an Azure function

This code can be manually deployed to an Azure function app.
Clone the repo locally and follow the [Azure functions guide to deploy the function](https://learn.microsoft.com/azure/azure-functions/functions-develop-vs-code?tabs=python). The chunking parameters can be customized to suit the needs of the content and retrieval scenarios.

Once the app has been published, make sure to [publish the application settings](https://learn.microsoft.com/azure/azure-functions/functions-develop-vs-code?tabs=python#publish-application-settings) with the required setting values filled in. Setting up the `local.settings.json` file from the previous section will make this fairly seamless.

## chunk-embed

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "1234",
            "data": {
                "document_id": "12345ABC",
                "text": "This is a test document and it is big enough to ensure that it meets the minimum chunk size.",
                "filepath": "foo.md",
                "fieldname": "content"
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
                "chunks": [
                    {
                        "content": "This is a test document and it is big enough to ensure that it meets the minimum chunk size.",
                        "id": null,
                        "title": "foo.md",
                        "filepath": "foo.md",
                        "url": null,
                        "embedding_metadata": {
                            "fieldname": "content",
                            "docid": "12345ABC",
                            "index": 0,
                            "offset": 0,
                            "length": 92,
                            "embedding": [
                                0.00544198,
                                0.006466314,
                                0.013019379,
                                . . . 
                            ]
                        }
                    }
                ],
                "total_files": 1,
                "num_unsupported_format_files": 0,
                "num_files_with_errors": 0,
                "skipped_chunks": 0
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
    "description": "Acronym linker",
    "uri": "[AzureFunctionEndpointUrl]/api/chunk-embed?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/content",
    "inputs": [
        {
            "name": "document_id",
            "source": "/document/document_id"
        },
        {
            "name": "text",
            "source": "/document/content"
        },
        {
            "name": "filepath",
            "source": "/document/file_path"
        },
        {
            "name": "fieldname",
            "source": "='content'"
        }
    ],
    "outputs": [
        {
            "name": "chunks",
            "targetName": "chunks"
        }
    ]
}
```

