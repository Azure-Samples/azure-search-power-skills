---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
name: Image store sample skills for cognitive search
urlFragment: azure-imagestore-sample
description: These custom skills store or retrieve a base64-encoded image to or from blob storage.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Vision/ImageStore/azuredeploy.json
---

# ImageStore

These custom skills store or retrieve a [base64](https://en.wikipedia.org/wiki/Base64)-encoded image to or from blob storage. This is useful to make images extracted from a cognitive search pipeline's data source available downstream as both blob URIs or raw base64 data, and to feed those into other skills.

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to [Azure blob storage](https://azure.microsoft.com/en-us/services/storage/blobs/).

## Settings

This function requires a `BLOB_STORAGE_CONNECTION_STRING` setting set to a valid Azure blob storage connection string, and a `BLOB_STORAGE_CONTAINER_NAME` setting set to the name of the blob storage container under which to save the new images.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FVision%2FImageStore%2Fazuredeploy.json)

## `image-store` function

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "logo",
            "data":
            {
                "imageName": "azure-logo.png",
                "mimeType": "image/png",
                "imageData": "iVBORw0KGgoAAAANSUhEUgAAAIIAAABgCAMAAADmUVpGAAAAolBMVEX///8ldLfC2etHisL8/f4ndbgpd7ktebrQ4fD5/P3a6PMmdbd3qdJTkcc9hL83gL05gb4zfbz2+vzw9vowe7vq8vjA1+uZv96JtNhBh8Hz9/vk7vZlns1Zlcje6vW40uivzeVOj8VLjMTt9PqSuttim8y61OmqyeOMttlqoM5fmcrV5fLL3u6dwd9+rdVvo9C0z+ahxODm7/emx+KFstfI3O0V9ET8AAADfUlEQVRo3u3ZaXOiMBzH8f9vuQTkBpV6tJ6rtl17vv+3tu0EVqvBUBJ0ZqefR4XpTBmOfJOU/hsZXVt/Qtdl7zy6rrnn3tNVOT40uqpbC+jRNS1cIGc/6nQN9g4f7tgVjOkKkik++Oxggt90cU6IT3129IaILu3WwqeAvQOxCXTpshZgBuywA8B8oQuyN2Aim51Y4oNHl5PMwJQ3f45Pbo8uxQlQsGJ2ZsUOfWrf/kVkRsTkYNZ0EWsXJXPOTg1RsGxqnz7C3paYR5SW1LpEw547KS4rRclwqGVOhANeeRZ7GrXr1sShDjFd7LWcirWBQzNibg5Pt5oKfYmvVsS840CbqUhyMCdPfYwvzBtqh5OCOXnoGY5MqRUrE0d8KtzhiPtALVi7ONanQohjISmnb1H9ZyY4tSDFbkKcGuznLqcsUmyLUykVbAMcY1LqFxj+x38ProwU0gOcMmMq7MDlk0KP4NhQIUGFJ1ImM3DKyKjQA4fiVHTBsaWSh72WUtEBh+tQ4QUcalOhT89/cq+oNiMl7sBUNEAPcUZHybhogakooYNzAiV1As+KSm84q0/SJuDRqBSb4FK4qliC5zeVHiCwJUk90dC7gYAxJCm2L3jAcwhIp+IZPIFOpSeI3UrFwQTPgP6ZQizSVccBqX00cor8kYiDOD43IcTMRG0cYCV0YJhCzFMWB2Z3dKsMCLmdhnGIwGO+8K+0jVQMwLXk/KLYY6M4uOAxnIp9nxZSMQLqrg5iDUIjqTiIl6tZAPWpiHNwTYnLMSGS0zf1wbcivp7yVGQWuDThNVdLJeMg3krrqk1Fp0H67bHKVOhek7lo4kunQjzkhjadM7RwnlE7FUkAvoHw+QkEknFAFAtvn6JUTIymb7S4WJYusXJgUxURfSORCvE4t6MaYk0+FXYOPrNeZ+aRdCoeZWvryKYiS8Hn3ktlfi+tFQfJjcznRqkQT4cfqDb9D4DGG1CekpWA7TVPxZ3UfEO8xhK/VnGgaudsaDRMxQCoPVWRKtZzZRxMVMh1+q4nnGFShZHSbbMFzlgSV0er4tkS/1vnMubUMr2z2GqRYAOqNc7zZhYaEOpRC+ZP3ZlvoSaLlIp7A89P8T1vpMj9YpxHBhowdJKVvW7ywERzM5n7vupqgQVpTrOPbT0LUxdqRN8NT38cpgaUeqXadKfz/qsN9OPHD46/rBY2v0REKtoAAAAASUVORK5CYII="
            }
        }
    ]
}
```

If unspecified or empty, `mimeType` defaults to "image/jpeg". If `imageName` is unspecified or empty, a new GUID is generated and used as the blob name.

### Sample Output:

```json
{
    "values": [
        {
            "recordId": "logo",
            "data": {
                "imageStoreUri": "https://[your storage account].blob.core.windows.net/pics/azure-logo.png"
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```

The returned `imageStoreUri` points to the image and can be used in its stead.

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "description": "Upload image data to the annotation store",
    "uri": "[AzureFunctionEndpointUrl]/api/image-store?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/normalized_images/*",
    "httpHeaders": {
        "BlobContainerName": "[BlobContainerName]"
    },
    "inputs": [
        {
            "name": "imageData",
            "source": "/document/normalized_images/*/data"
        }
    ],
    "outputs": [
        {
            "name": "imageStoreUri",
            "targetName": "imageStoreUri"
        }
    ]
}
```

## `image-fetch` function

This function is the reverse of `image-store`, and the inputs and outputs are identical, just reversed, if one excludes the errors and warnings sections.

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "logo",
            "data": {
                "imageStoreUri": "https://[your storage account].blob.core.windows.net/pics/azure-logo.png"
            }        }
    ]
}
```

### Sample Output:

```json
{
    "values": [
        {
            "recordId": "logo",
            "data":
            {
                "imageName": "azure-logo.png",
                "mimeType": "image/png",
                "imageData": "iVBORw0KGgoAAAANSUhEUgAAAIIAAABgCAMAAADmUVpGAAAAolBMVEX///8ldLfC2etHisL8/f4ndbgpd7ktebrQ4fD5/P3a6PMmdbd3qdJTkcc9hL83gL05gb4zfbz2+vzw9vowe7vq8vjA1+uZv96JtNhBh8Hz9/vk7vZlns1Zlcje6vW40uivzeVOj8VLjMTt9PqSuttim8y61OmqyeOMttlqoM5fmcrV5fLL3u6dwd9+rdVvo9C0z+ahxODm7/emx+KFstfI3O0V9ET8AAADfUlEQVRo3u3ZaXOiMBzH8f9vuQTkBpV6tJ6rtl17vv+3tu0EVqvBUBJ0ZqefR4XpTBmOfJOU/hsZXVt/Qtdl7zy6rrnn3tNVOT40uqpbC+jRNS1cIGc/6nQN9g4f7tgVjOkKkik++Oxggt90cU6IT3129IaILu3WwqeAvQOxCXTpshZgBuywA8B8oQuyN2Aim51Y4oNHl5PMwJQ3f45Pbo8uxQlQsGJ2ZsUOfWrf/kVkRsTkYNZ0EWsXJXPOTg1RsGxqnz7C3paYR5SW1LpEw547KS4rRclwqGVOhANeeRZ7GrXr1sShDjFd7LWcirWBQzNibg5Pt5oKfYmvVsS840CbqUhyMCdPfYwvzBtqh5OCOXnoGY5MqRUrE0d8KtzhiPtALVi7ONanQohjISmnb1H9ZyY4tSDFbkKcGuznLqcsUmyLUykVbAMcY1LqFxj+x38ProwU0gOcMmMq7MDlk0KP4NhQIUGFJ1ImM3DKyKjQA4fiVHTBsaWSh72WUtEBh+tQ4QUcalOhT89/cq+oNiMl7sBUNEAPcUZHybhogakooYNzAiV1As+KSm84q0/SJuDRqBSb4FK4qliC5zeVHiCwJUk90dC7gYAxJCm2L3jAcwhIp+IZPIFOpSeI3UrFwQTPgP6ZQizSVccBqX00cor8kYiDOD43IcTMRG0cYCV0YJhCzFMWB2Z3dKsMCLmdhnGIwGO+8K+0jVQMwLXk/KLYY6M4uOAxnIp9nxZSMQLqrg5iDUIjqTiIl6tZAPWpiHNwTYnLMSGS0zf1wbcivp7yVGQWuDThNVdLJeMg3krrqk1Fp0H67bHKVOhek7lo4kunQjzkhjadM7RwnlE7FUkAvoHw+QkEknFAFAtvn6JUTIymb7S4WJYusXJgUxURfSORCvE4t6MaYk0+FXYOPrNeZ+aRdCoeZWvryKYiS8Hn3ktlfi+tFQfJjcznRqkQT4cfqDb9D4DGG1CekpWA7TVPxZ3UfEO8xhK/VnGgaudsaDRMxQCoPVWRKtZzZRxMVMh1+q4nnGFShZHSbbMFzlgSV0er4tkS/1vnMubUMr2z2GqRYAOqNc7zZhYaEOpRC+ZP3ZlvoSaLlIp7A89P8T1vpMj9YpxHBhowdJKVvW7ywERzM5n7vupqgQVpTrOPbT0LUxdqRN8NT38cpgaUeqXadKfz/qsN9OPHD46/rBY2v0REKtoAAAAASUVORK5CYII="
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
    "description": "Upload image data to the annotation store",
    "uri": "[AzureFunctionEndpointUrl]/api/image-fetch?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document/normalized_images/*",
    "httpHeaders": {
        "BlobContainerName": "[BlobContainerName]"
    },
    "inputs": [
        {
            "name": "imageStoreUri",
            "source": "/document/normalized_images/*/uri"
        }
    ],
    "outputs": [
        {
            "name": "imageData",
            "targetName": "data"
        }
    ]
}
```
