# custom_ner_skill
---
Description:
- It is common to have custom entities along different texts that dont fit any of the predefined entities that can be extracted with Named Entity Extraction service. Custom Named Entity Recognition (in preview as of Nov2021) provides the capability to ingest your training texts, label your set of custom entities and train a model to identify them. You can easily deploy the model in a secured fashion to later on run your inference along your texts. As an outcome you will get the detected custom entities, their position (inside the text) and the confidence level

- custom_ner_skill is an Azure Cognitive Search skill to integrate Azure Text Analytics Custom Named Entity Recoginition within a Azure Cognitive Search skillset. This will enable the cracking of documents in a programmatic way to enrich your search with different custom entities. For example, show me the loan documents signed with the credit institution X between May and June 2021 with higher purchase amount than one millon dollars. This filtering is possible because Text Analytics has identified all those fields along the skillset execution and exposes the ability to narrow the results within the ACS index.

Languages:
- ![python](https://img.shields.io/badge/language-python-orange)

Products:
- Azure Cognitive Search
- Azure Cognitive Services (Text Analytics)
- Azure Functions
---

# Steps    

1. Create or reuse a Text Analytics resource. Creation can be done from the Azure portal or in [Language Studio](https://language.azure.com/home)
2. Train your model with a dataset (a sample train and eval dataset can be found [here](https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/language-service/Custom%20NER/loan%20agreements) in case you dont have docs to work with) and deploy it. In case you are not familiar with Custom NER, this is a simple [tutorial](https://docs.microsoft.com/en-us/azure/cognitive-services/language-service/custom-named-entity-recognition/quickstart?pivots=language-studio#upload-sample-data-to-blob-container) to guide you
3. Fill your appsettings with the custom details from your deployment ('TA_ENDPOINT', 'TA_KEY', 'DEPLOYMENT', 'PROJECT_NAME')
4. Clone this repository
5. Open the folder in VS Code and deploy the function, find here a [tutorial](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-python)
6. Add a field in your index where you will dump the enriched entities, more info [here](#sample-index-field-definition)
7. Add the skill to your skillset as [described below](#sample-skillset-integration)
8. Add the output field mapping in your indexer as [seen in the sample](#sample-indexer-output-field-mapping)
9. Run the process 

## Sample Input:

You can find a sample input for the skill [here](../main/custom_ner/sample.dat)

```json
{
    "values": [
      {
        "recordId": "0",
        "data":
           {
            "text": "Date 10/18/2019 This is a Loan agreement between the two individuals mentioned below in the parties section of the agreement. I. Parties of agreement - Casey Jensen with a mailing address of 2469 Pennsylvania Avenue, City of New Brunswick, State of New Jersey (the Borrower) - Hollie Rees with a mailing address of 42 Gladwell Street, City of Memphis, State of Tennessee (the Lender) II. Amount The loan amount given by lender to borrower is one hundred ninety-two thousand nine hundred eighty-nine Dollars ($192,989.00) (The Note)",
            "lang": "en"
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
      "recordId": "0",
      "data": {
        "text": {
          "text": "$192,989.00)",
          "category": "Quantity",
          "offset": 482,
          "length": 12,
          "confidenceScore": 1
        }
      }
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
      "name": "Text Analytics Custom NER",
      "description": "Extract your custom entities",
      "context": "/document",
      "uri": "https://x.azurewebsites.net/api/y?code=z==",
      "httpMethod": "POST",
      "timeout": "PT30S",
      "batchSize": 1,
      "degreeOfParallelism": null,
      "inputs": [
        {
          "name": "lang",
          "source": "/document/language"
        },
        {
          "name": "text",
          "source": "/document/corpus"
        }
      ],
      "outputs": [
        {
          "name": "text",
          "targetName": "entities"
        }
      ],
      "httpHeaders": {}
    }
```

## Sample Index Field Definition

The skill will output the entities that have been extracted for the corpus. In this example, I am just expecting one entity so I need to populate a field of Edm.ComplexType that will contain subfields for Category, Confidence, Offset and Length. If more than one Entity is expected, go for Collection.ComplexType instead of Edm.ComplexType.

```json
{
      "name": "entity",
      "type": "Edm.ComplexType",
      "analyzer": null,
      "synonymMaps": [],
      "fields": [
        {
          "name": "text",
          "type": "Edm.String",
          "facetable": false,
          "filterable": false,
          "key": false,
          "retrievable": true,
          "searchable": true,
          "sortable": false,
          "analyzer": "standard.lucene",
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        },
        {
          "name": "category",
          "type": "Edm.String",
          "facetable": true,
          "filterable": true,
          "key": false,
          "retrievable": true,
          "searchable": true,
          "sortable": false,
          "analyzer": "standard.lucene",
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        },
        {
          "name": "confidence",
          "type": "Edm.Double",
          "facetable": false,
          "filterable": false,
          "retrievable": true,
          "sortable": true,
          "analyzer": null,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        },
        {
          "name": "offset",
          "type": "Edm.String",
          "facetable": false,
          "filterable": false,
          "key": false,
          "retrievable": true,
          "searchable": true,
          "sortable": false,
          "analyzer": "standard.lucene",
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        },
        {
          "name": "length",
          "type": "Edm.String",
          "facetable": false,
          "filterable": false,
          "key": false,
          "retrievable": true,
          "searchable": true,
          "sortable": false,
          "analyzer": "standard.lucene",
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        }
      ]
    }
```

## Sample Indexer Output Field Mapping

The output enrichment of your skill can be directly mapped to one of your fields described above. This can be done with the indexer setting:
```
  "outputFieldMappings": [
    {
      "sourceFieldName": "/document/entities",
      "targetFieldName": "entity"
    }
```
