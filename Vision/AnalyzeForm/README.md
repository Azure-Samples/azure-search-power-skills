---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
- azure-cognitive-services
name: Analyze form sample skill for cognitive search
urlFragment: azure-analyzeform-sample
description: This custom skill extracts specific fields from the results of a trained form recognition.
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/master/Vision/AnalyzeForm/azuredeploy.json
---

# AnalyzeForm

This custom skill extracts specific fields from the results of a trained form recognition.

A [full tutorial on this skill is available in the Azure Cognitive Seach documentation](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-form).

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Forms Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. At the time this template was written, Forms Recognizer was in a gated public preview. If you have not done so, you may need to [request access](https://aka.ms/FormRecognizerRequestAccess).

You will need to [train a model with your forms](https://docs.microsoft.com/en-us/azure/cognitive-services/form-recognizer/quickstarts/curl-train-extract) before you can use this skill. The model that was used for this example was trained using sample data that can be downloaded from [the SampleData directory](https://github.com/Azure-Samples/azure-search-power-skills/tree/master/SampleData).

## Settings

This function requires a `FORMS_RECOGNIZER_API_KEY` and a `FORMS_RECOGNIZER_ENDPOINT_URL` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.0-preview endpoint.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

After training, you will need to set the `FORMS_RECOGNIZER_MODEL_ID` application setting to the model id corresponding to your trained model.

By default, the skill will retry at most a hundred times getting form recognition results with a one second delay between attempts until it gets a result other than "running".
This can be changed by setting the `FORMS_RECOGNIZER_MAX_ATTEMPTS` and `FORMS_RECOGNIZER_RETRY_DELAY` application settings.

The list of fields to extract and the fields they get mapped to in the response of the skill need to be configured to reflect your particular scenario. This can be done by editing [the `field-mappings.json` file](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Vision/AnalyzeForm/field-mappings.json).

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FVision%2FAnalyzeForm%2Fazuredeploy.json)

## Sample Input:

This sample data is pointing to a file stored in this repository, but when the skill is integrated in a skillset, the URL and token will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "formUrl": "https://github.com/Azure-Samples/azure-search-power-skills/raw/master/SampleData/Invoice_4.pdf",
                "formSasToken":  "?st=sasTokenThatWillBeGeneratedByCognitiveSearch"
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
                "address": "1111 8th st. Bellevue, WA 99501 ",
                "recipient": "Southridge Video 1060 Main St. Atlanta, GA 65024 "
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
    "name": "formrecognizer", 
    "description": "Extracts fields from a form using a pre-trained form recognition model",
    "uri": "[AzureFunctionEndpointUrl]/api/analyze-form?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT30S",
    "context": "/document",
    "batchSize": 1,
    "inputs": [
        {
            "name": "formUrl",
            "source": "/document/metadata_storage_path"
        },
        {
            "name": "formSasToken",
            "source": "/document/metadata_storage_sas_token"
        }
    ],
    "outputs": [
        {
            "name": "address",
            "targetName": "address"
        },
        {
            "name": "recipient",
            "targetName": "recipient"
        }
    ]
}
```
