---
page_type: sample
languages:
- python
products:
- azure
- azure-cognitive-search
- azure-cognitive-services
- azure-form-recognizer
name: Analyze document using the different Form Recognizer APIs
urlFragment: azure-formrecognizer-sample
description: This custom skill can extract OCR text, tables, key value pairs and custom fomr fields from a document. 
---
Invoking a Form Recognizer capability within the Cognitive Search pipeline is now merged into a single skill.
* [Analyze Document](#AnalyzeDocument), using a pre built model or a custom model
Supported models include:
- Layout (No training required)
- Prebuilt models (No training required)
    - Invoices
    - Receipts
    - Id document
    - Business Cards
- General Document (No training required)
- Custom Form


# Deployment    

The analyze form skill enables you to use a pretrained model or a custom model to identify and extract key value pairs, entities and tables. The skill requires the `FORMS_RECOGNIZER_ENDPOINT` and `FORMS_RECOGNIZER_KEY` property set in the appsettings to the appropriate Form Recognizer resource endpoint and key.

To deploy the skills:
1. In the Azure portal, create a Forms Recognizer resource.
2. Copy the form recognizer URL and key for use in the training and appsettings.
3. Clone this repository
4. Open the FormRecognizer folder in VS Code and deploy the function.
5. Once the function is deployed, set the required appsettings (`FORMS_RECOGNIZER_ENDPOINT`, `FORMS_RECOGNIZER_KEY`).  On the Azure portal, these can be found in your Azure function in the "Configuration" page under the "Settings" section.  Add them as new Application settings.  See [here](https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal#settings) for further description.  
6. (Optional) To use a custom form, follow the [tutorial](https://docs.microsoft.com/en-us/azure/applied-ai-services/form-recognizer/quickstarts/try-v3-form-recognizer-studio) to train a custom model in the [Form Recognizer Studio](https://formrecognizer.appliedai.azure.com/studio)
7. Add the skill to your skillset as [described below](#sample-skillset-integration)

# AnalyzeDocument

This custom skill can invoke any of the following Form Recognizer APIs
1. Layout
2. Prebuilt invoice
3. Prebuilt receipt
4. Prebuilt ID
5. Prebuilt business card
6. General document
7. Custom form

## Requirements

In addition to the common requirements described in [the root `README.md` file](../../README.md), this function requires access to an [Azure Form Recognizer](https://azure.microsoft.com/en-us/services/cognitive-services/form-recognizer/) resource. 

[Train a model with your forms](https://docs.microsoft.com/en-us/azure/applied-ai-services/form-recognizer/build-training-data-set) before you can use this skill. As a sample to get started, use the included [sample training forms](Train) and [sample test form](Test) with the [training notebook](FormRecognizerTrainModel.ipynb) to create a model in just a few minutes.

## Settings

This function requires a `FORMS_RECOGNIZER_ENDPOINT` and a `FORMS_RECOGNIZER_KEY` settings set to a valid Azure Forms Recognizer API key and to your custom Form Recognizer 2.1-preview endpoint. 
If running locally, this can be set in your project's local environment variables. This ensures your key won't be accidentally checked in with your code.
If running in an Azure function, this can be set in the application settings.

## Sample Input:

This sample data is pointing to a file stored in this repository, but when the skill is integrated in a skillset, the URL and token will be provided by cognitive search.

```json
{
    "values": [
        {
            "recordId": "record1",
            "data": { 
                "model": "prebuilt-invoice",
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
    "uri": "[AzureFunctionEndpointUrl]/api/AnalyzeDocument?code=[AzureFunctionDefaultHostKey]",
    "httpMethod": "POST",
    "timeout": "PT1M",
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
        },
        {
          "name": "model",
          "source": "= 'prebuilt-invoice'"
        }
    ],
    "outputs": [
        {
            "name": "fields",
            "targetName": "fields"
        },
        {
            "name": "tables",
            "targetName": "tables"
        }
    ]
}
```

