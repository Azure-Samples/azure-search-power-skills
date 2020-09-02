---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-key-vault
- azure-storage
name: Decrypt blob file sample skill for cognitive search
urlFragment: azure-decryptblob-sample
description: This custom skill downloads and decrypts a file that was encrypted in Azure Blob Storage and returns it back to Azure Cognitive Search to be indexed.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Utils/DecryptBlobFile/azuredeploy.json
---

# DecryptBlobFile

This custom skill downloads and decrypts a file that was encrypted in Azure Blob Storage and returns it back to Azure Cognitive Search to be processed and indexed. It is meant to be used in combination with the built-in [DocumentExtractionSkill](https://docs.microsoft.com/azure/search/cognitive-search-skill-document-extraction) to allow you to index encrypted files without needing to worry about them being stored unecrypted at rest. For more details on how to encrypt files in blob storage, [see this tutorial](https://docs.microsoft.com/azure/storage/blobs/storage-encrypt-decrypt-blobs-key-vault).

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires key get access to the [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) resource where the key that was used to encrypt the files stored in Azure Blob Storage lives. This access should be granted by [setting an access policy on the Key Vault](https://docs.microsoft.com/azure/key-vault/general/assign-access-policy-portal) with the principal being the Azure Function instance that the skill is deployed to.

## Settings

This function doesn't require any application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FUtils%2FDecryptBlobFile%2Fazuredeploy.json)

## Sample Input:

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "blobUrl": "http://blobStorage.com/myencryptedfile",
                "sasToken": "?sas=123&otherSasInfo=456"
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
            "recordId": "record1",
            "data": {
                "decrypted_file_data": {
                    "$type": "file",
                    "data": "<base64 encoded decrypted file data>"
                }
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
    "name": "decryptblobfile", 
    "description": "Downloads and decrypts a file that was encrypted in Azure Blob Storage",
    "uri": "[AzureFunctionEndpointUrl]/api/decrypt-blob-file?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT30S",
    "context": "/document",
    "batchSize": 1,
    "inputs": [
        {
            "name": "blobUrl",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "sasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "decrypted_file_data",
            "targetName": "decrypted_file_data"
        }
    ]
}
```

It is suggested to follow up this custom skill with a DocumentExtractionSkill that looks like the following:

```json
{
    "@odata.type": "#Microsoft.Skills.Util.DocumentExtractionSkill",
    "parsingMode": "default",
    "dataToExtract": "contentAndMetadata",
    "context": "/document",
    "inputs": [
        {
            "name": "file_data",
            "source": "/document/decrypted_file_data"
        }
    ],
    "outputs": [
        {
            "name": "content",
            "targetName": "extracted_content"
        }
    ]
}
```

It is also suggested to add the [configuration parameter](https://docs.microsoft.com/rest/api/searchservice/create-indexer#parameters) `"dataToExtract": "storageMetadata"` to your indexer definition when running an indexer with this skill. This ensures that the indexer does not fail before the skillset is given a chance to execute, and the content and metadata that would normally be extracted with the default `dataToExtract` option `contentAndMetadata` will be extracted instead by the DocumentExtractionSkill.