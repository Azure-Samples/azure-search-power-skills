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

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Forms Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. At the time this template was written, Forms Recognizer was in a gated public preview. If you have not done so, you may need to [request access](https://aka.ms/FormRecognizerRequestAccess).

You will need to [train a model with your forms](https://docs.microsoft.com/en-us/azure/cognitive-services/form-recognizer/quickstarts/curl-train-extract) before you can use this skill. The model that was used for this example was trained using sample data that can be downloaded from [the SampleData directory](https://github.com/Azure-Samples/azure-search-power-skills/tree/master/SampleData).

## Settings

This function requires a `FORMS_RECOGNIZER_API_KEY` setting set to a valid Azure Forms Recognizer API key.
If running locally, this can be set in your project's debug environment variables (go to project properties, in the debug tab). This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

After training, you will need to set the `FORMS_RECOGNIZER_MODEL_ID` application setting to the model id corresponding to your trained model.

The list of fields to extract and the fields they get mapped to in the response of the skill need to be configured to reflect your particular scenario. This can be done by editing [the `fieldMappings` dictionary in the `AnalyzeForm.cs` file](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Vision/AnalyzeForm/AnalyzeForm.cs#L24).

This example was written to deal with PDF files, but if you are working different file types, you may change the content-type sent to the forms recognizer [by modifying the `contentType` constant in the `AnalyzeForm.cs` file](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Vision/AnalyzeForm/AnalyzeForm.cs#L29).

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
