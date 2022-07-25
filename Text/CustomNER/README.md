# custom_ner_skill

---

Description:

- It is common to have custom entities along different texts that don't fit any of the predefined entities that can be extracted with the Named Entity Extraction service. Custom Named Entity Recognition provides the capability to ingest your training texts, label your set of custom entities and train a model to identify them. You can easily deploy the model in a secured fashion to later on run your inference along your texts. As an outcome you will get the detected custom entities, their position (inside the text) and the confidence level

- custom_ner_skill is an Azure Cognitive Search skill to integrate Azure Text Analytics Custom Named Entity Recognition within a Azure Cognitive Search skillset. This will enable the cracking of documents in a programmatic way to enrich your search with different custom entities. For example, show me the loan documents signed with the credit institution X between May and June 2021 with higher purchase amount than one million dollars. This filtering is possible because Text Analytics has identified all those fields along the skillset execution and exposes the ability to narrow the results within the ACS index.

Languages:

- ![python](https://img.shields.io/badge/language-python-orange)

Products:

- Azure Cognitive Search
- Azure Cognitive Services for Language (Text Analytics)
- Azure Functions

Table of Contents:

* [Steps](#steps)
  
  * [Create or reuse a Custom NER project](#create-or-reuse-a-custom-ner-project)
  - [Deploy the powerskill to Azure](#deploy-the-powerskill-to-azure)
  
  - [Integrate with Azure Cognitive Search](integrate-with-azure-cognitive-search)
    
    - [Skillset](#skillset)
    
    - [Index](#index)
    
    - [Indexer](#indexer)
- [Automated Deployment](#automated-deployment)

- [Testing](#testing)

---

# Steps

#### Create or reuse a Custom NER project

In order to use Custom NER, we need a language resource and a trained (and deployed) project to be used in recognizing the custom entities. If they aren't previously created, now is the time to do that. A good place to start is [Quickstart - Custom named entity recognition (NER) - Azure Cognitive Services | Microsoft Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/custom-named-entity-recognition/quickstart?pivots=language-studio).

After this step you should have:

* A Language resource.

* A project

* A deployment

#### Deploy the powerskill to Azure

A powerskill is basically just an Azure Function written to be used as a custom skill in an Azure Cognitive Search pipeline. To deploy a function, an Azure App resource is needed. Notice the difference between an Azure Function and an Azure App. If one is not already available, now is the time to create one. A good article that goes through all the steps of creating a simple Azure Function in Python is [Create a Python function using Visual Studio Code - Azure Functions | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/create-first-function-vs-code-python).

After creating the resource, some app settings (visible inside Azure Functions as environment variables) need to be added for the powerskill to run correctly.

| App setting     | Description                                                     | Details                                                                                                      |
| --------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| TA_ENDPOINT     | The Language resource endpoint.                                 | E.g. https://<language-resource-name>.cognitiveservices.azure.com/                                           |
| TA_KEY          | The access key to be able to access the endpoint.               | Can be found under `Resource Management -> Keys and Endpoint` in the Language resource page in Azure Portal. |
| PROJECT_NAME    | The name of the created project in the previous step.           |                                                                                                              |
| DEPLOYMENT_NAME | The name of the deployment of the project in the previous step. |                                                                                                              |

After creating an Azure App resource, the Custom NER powerskill can be deployed to the Azure App. This is commonly done in two ways. One approach is to create a local project in Visual Studio Code and deploy it using the Azure Functions extension (more details can be found at the previously linked article). Another approach is to do a __Zip Deploy__ i.e. upload a zipped file containing the function code and configuration (with a similar structure to [this](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference-python#folder-structure)). An app that is Zip-Deployed can be setup to either [run from a package file](https://docs.microsoft.com/en-us/azure/azure-functions/run-functions-from-deployment-package) or [do a remote build](https://docs.microsoft.com/en-us/azure/azure-functions/run-functions-from-deployment-package). Running from a package assumes that the project is ready to be run and skips doing any build steps (e.g. `npm install`, or in this case, `pip install`). Since this Python project needs to pull dependencies with pip, a remote build is the more appropriate choice. A simple command to zip the necessary files would be

```bash
 zip -r customner-powerskill.zip custom_ner host.json requirements.txt
```

For more information on deployment methods, see [Deployment technologies in Azure Functions | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-deployment-technologies).

At this point, you should have a working Azure Function. Depending on how you deployed the function, you may need to add a `x-functions-key` header to each request to the function endpoint. The value for the header can be found in `Functions -> App keys` in the Function App resource page in Azure Portal.

The function adheres to the input/output format specified by Azure Cognitive Search for custom skills. More information about custom skills and format, see [Custom skill interface - Azure Cognitive Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-interface). Sample inputs and outputs for this powerskill are shown below. Notice that specifying the language is optional, and defaults to English.

```json
{
    "values": [
      {
        "recordId": "0",
        "data":
           {
            "text":"Date 10/18/2019\n\nThis is a Loan agreement between the two individuals mentioned below in the parties section of the agreement.\n\nI. Parties of agreement\n\n- Casey Jensen with a mailing address of 2469 Pennsylvania Avenue, City of New Brunswick, State of New Jersey (the \"Borrower\")\n- Hollie Rees with a mailing address of 42 Gladwell Street, City of Memphis, State of Tennessee (the \"Lender\")\n\nII. Amount\nThe loan amount given by lender to borrower is one hundred ninety-two thousand nine hundred eighty-nine Dollars ($192,989.00) (\"The Note\")\n\nIII. Interest\nThe Note shall bear interest five percent (5%) compounded annually.\n\nIV. Payment\nThe amount mentioned in this agreement (the \"Note\"), including the principal and any accrued interest, is\n\nV. Payment Terms\nAny delay in payment is subject to a fine with a flat amount of $50 for every week the payment is delayed.\nAll payments made by the Borrower shall be go into settling the the accrued interest  and any late fess and then into the payment of the principal amount.\n\nVI. Prepayment\nThe borrower is able to pay back the Note in full at any time, thus terminating this agreement.\nThe borrower also can make additional payments at any time and this will take of from the amount of the latest installments. \n\nVII. Acceleration.\nIn case of Borrower's failure to pay any part of the principal or interest as and when due under this Note; or Borrower's becoming insolvent or not paying its debts as they become due. The lender has the right to declare an \"Event of Acceleration\" in which case the Lender has the right to to declare this Note immediately due and payable \n\nIX. Succession\nThis Note shall outlive the borrower and/or the lender in the even of their death. This note shall be binging to any of their successors.",
            "language":"en"
           }
      }
    ]
}
```

```json
{
    "values": [
        {
            "recordId": "0",
            "data": {
                "entities": [
                    {
                        "text": "10/18/2019",
                        "category": "Date",
                        "subcategory": null,
                        "length": 10,
                        "offset": 5,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "Casey Jensen",
                        "category": "BorrowerName",
                        "subcategory": null,
                        "length": 12,
                        "offset": 155,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "2469 Pennsylvania Avenue",
                        "category": "BorrowerAddress",
                        "subcategory": null,
                        "length": 24,
                        "offset": 194,
                        "confidence_score": 0.99
                    },
                    {
                        "text": "New Brunswick",
                        "category": "BorrowerCity",
                        "subcategory": null,
                        "length": 13,
                        "offset": 228,
                        "confidence_score": 0.95
                    },
                    {
                        "text": "New Jersey",
                        "category": "BorrowerState",
                        "subcategory": null,
                        "length": 10,
                        "offset": 252,
                        "confidence_score": 0.81
                    },
                    {
                        "text": "Hollie Rees",
                        "category": "LenderName",
                        "subcategory": null,
                        "length": 11,
                        "offset": 282,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "42 Gladwell Street",
                        "category": "LenderAddress",
                        "subcategory": null,
                        "length": 18,
                        "offset": 320,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "Memphis",
                        "category": "LenderCity",
                        "subcategory": null,
                        "length": 7,
                        "offset": 348,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "Tennessee",
                        "category": "LenderState",
                        "subcategory": null,
                        "length": 9,
                        "offset": 366,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "one hundred ninety-two thousand nine hundred eighty-nine Dollars",
                        "category": "LoanAmountWords",
                        "subcategory": null,
                        "length": 64,
                        "offset": 450,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "$192,989.00",
                        "category": "LoanAmountNumbers",
                        "subcategory": null,
                        "length": 11,
                        "offset": 516,
                        "confidence_score": 1.0
                    },
                    {
                        "text": "5%",
                        "category": "Interest",
                        "subcategory": null,
                        "length": 2,
                        "offset": 600,
                        "confidence_score": 1.0
                    }
                ]
            },
            "warnings": []
        }
    ]
}
```

#### Integrate with Azure Cognitive Search

###### Skillset

An Azure Cognitive Search pipeline consists of an Index, an Indexer, a skillset and data source. This function can be used as a custom skill in a skillset (either as a singular custom skill in a skillset, or as a skill among many others in a skillset). To add this function as a custom skill, some parameters need to be specified, including the the endpoint URL of the Function App deployed in the previous steps, the `x-functions-key` header, what is needed as input, and what the output is named. An example is shown below. Here, the text of each document (`/document/content`) is sent to the api as a value for the key named `text` (as the function expects) and the output is the value of the key named `entities` in the response. An example of what a skillset may look like is shown below. For more information on skillsets, see [Skillset concepts - Azure Cognitive Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/cognitive-search-working-with-skillsets).

```json
{
    "skills": [
      "[... your existing skills remain here]",  
      {
        "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
        "name": "customner-skill",
        "description": "",
        "context": "/document",
        "uri": "https://<azure-function-name>.azurewebsites.net/api/custom_ner",
        "httpMethod": "POST",
        "timeout": "PT30S",
        "batchSize": 1,
        "degreeOfParallelism": 1,
        "inputs": [
          {
            "name": "text",
            "source": "/document/content"
          }
        ],
        "outputs": [
          {
            "name": "entities",
            "targetName": "entities"
          }
        ],
        "httpHeaders": {
          "x-functions-key": "<azure-function-key>"
        }
      }
  ]
}
```

###### Index

Next, the output from the custom skill can be used as an input to yet another skill, or be part of the final output that is saved into the index. Assuming the latter, the index needs to have a field definition that matches the output it's given. An example for what the definition should look like is shown below. For more information on search indexes, see [Index overview - Azure Cognitive Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/search-what-is-an-index).

```json
{
      "name": "entities",
      "type": "Collection(Edm.ComplexType)",
      "fields": [
        {
          "name": "text",
          "type": "Edm.String",
          "searchable": true,
          "filterable": false,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": "standard.lucene",
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "category",
          "type": "Edm.String",
          "searchable": true,
          "filterable": false,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": "standard.lucene",
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "subcategory",
          "type": "Edm.String",
          "searchable": true,
          "filterable": false,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": "standard.lucene",
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "length",
          "type": "Edm.Int64",
          "searchable": false,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": null,
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "offset",
          "type": "Edm.Int64",
          "searchable": false,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": null,
          "normalizer": null,
          "synonymMaps": []
        },
        {
          "name": "confidence_score",
          "type": "Edm.Double",
          "searchable": false,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "facetable": false,
          "key": false,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "analyzer": null,
          "normalizer": null,
          "synonymMaps": []
        }
      ]
    }
```

###### Indexer

Finally, the indexer ties everything together. The indexer needs to be setup up such that the outputs from the custom skill are mapped to the field that was just defined. Notice the `outputFieldMappings` key in the example shown below, the content of each document is mapped to the a field named `textBody`, and the output from the powerskill (that was named `entities` in the skillset) is mapped to a field named `entities` in the index. For more information on output mappings, see [Map skill output fields - Azure Cognitive Search | Microsoft Docs](https://docs.microsoft.com/en-us/azure/search/cognitive-search-output-field-mapping) in addition to the previously linked article about skillsets.

```json
{
  "@odata.context": "...",
  "@odata.etag": "...",
  "name": "<indexer-name>",
  "dataSourceName": "<datasource-name>",
  "skillsetName": "<skillset-name>",
  "targetIndexName": "<index-name>",
  ...
  "outputFieldMappings": [
    {
      "sourceFieldName": "/document/content",
      "targetFieldName": "textBody"
    },
    {
      "sourceFieldName": "/document/entities",
      "targetFieldName": "entities"
    }
  ]
}
```

## Automating deployment

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fcustom_ner_revamp%2FText%2FCustomNER%2Fazuredeploy.json)

As an alternative to doing the previous steps, an ARM template (see [Templates overview - Azure Resource Manager | Microsoft Docs](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/overview)) is provided to automate creating the Function App resource and deploying this powerskill on the resource. The ARM template requires that the Azure Functions project to be deployed is zipped and uploaded to an accessible location. The ARM template deploys the zip fle to the Function App resource it creates (see [Deploy the powerskill to Azure](#deploy-the-powerskill-to-Azure)). The zip can, for example, be uploaded to Azure Blob Storage, and its URL can be given to the ARM template. The ARM template also takes the app settings that the powerskill needs (`TA_ENDPOINT`, etc.).

## Testing

A small test suite is provided in the `tests` directory. The tests assume a `.env` file that contains the parameters that the powerskill needs to run. The file uses a simple `KEY=VAL` syntax separated by newlines.

```
PROJECT_NAME=<project-name>
DEPLOYMENT_NAME=<deployment-name>
TA_KEY=<language-resource-key>
TA_ENDPOINT=https://<language-resource-name>.cognitiveservices.azure.com
```

The test cases assume that the model is trained to recognize load agreements like the example in [Quickstart - Custom named entity recognition (NER) - Azure Cognitive Services | Microsoft Docs](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/custom-named-entity-recognition/quickstart?pivots=language-studio). Training and test data can be found [here](https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/language-service/Custom%20NER).

To run the tests, `cd` into the root of the project and run `unittest`.

```bash
cd CustomNER
python -m unittest
```
