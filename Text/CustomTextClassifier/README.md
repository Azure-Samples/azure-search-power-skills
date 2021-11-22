# text_classification_skill
---
Description:
- It is common to require a text classification along knowledge base scenarios, for example you might to want to classify a document as a RFI response, a contract, a letter of intent or just a BoM. Custom Text Classification (in preview as of Nov2021) provides the capability to ingest your training texts, label your set of custom labels (both single and multi class) and train a model to classify them. You can easily deploy the model in a secured fashion to later on run your inference along your texts. As an outcome you will get the detected custom classes and the confidence level

- text_classification_skill is an Azure Cognitive Search skill to integrate [Azure Text Analytics Custom Text Classification](https://docs.microsoft.com/azure/cognitive-services/language-service/custom-classification/overview) within a Azure Cognitive Search skillset. This will enable the cracking of documents in a programmatic way to enrich your search with different custom classes. For example, show me the RFI responses by X employee between May and June 2021. This filtering is possible because Text Analytics has identified all those classes along the skillset execution and exposes the ability to narrow the results within the ACS index.

Languages:
- ![python](https://img.shields.io/badge/language-python-orange)

Products:
- Azure Cognitive Search
- Azure Cognitive Services (Text Analytics)
- Azure Functions
---

# Steps    

1. Create or reuse a Text Analytics resource. Creation can be done from the Azure portal or in [Language Studio](https://language.azure.com/home)
2. Train your model with a dataset (a sample train and eval dataset can be found [here](https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/language-service/Custom%20text%20classification/movies%20summaries) in case you dont have docs to work with) and deploy it. In case you are not familiar with Custom Text Classification, this is a simple [tutorial](https://docs.microsoft.com/azure/cognitive-services/language-service/custom-classification/quickstart?pivots=language-studio) to guide you
3. Create a Python Function in Azure, for example this is a good [starting point](https://docs.microsoft.com/azure/azure-functions/create-first-function-vs-code-python)
4. Clone this repository
5. Open the folder in VS Code and deploy the function, find here a [tutorial](https://docs.microsoft.com/azure/search/cognitive-search-custom-skill-python)
6. Fill your Functions appsettings with the custom details from your deployment ('TA_ENDPOINT', 'TA_KEY', 'DEPLOYMENT', 'PROJECT_NAME' with the info you got in Language Studio after you deployed the model
7. Add a field in your index where you will dump the enriched classes, more info [here](#sample-index-field-definition)
8. Add the skill to your skillset as [described below](#sample-skillset-integration)
9. Add the output field mapping in your indexer as [seen in the sample](#sample-indexer-output-field-mapping)
10. Run the indexer 

## Sample Input:

You can find a sample input for the skill [here](../main/custom_ner/sample.dat)

```json
{
    "values": [
      {
        "recordId": "0",
        "data":
           {
            "text": "Set in todays Mumbai, Barah Aana revolves around three friends: Shukla, a driver, Yadav, a watchman, and Aman, a waiter. Shukla is an older man, stoic and steady. Yadav, in his 30s, is meek and something of a pushover at work, but exhibits an underlying mischievous nature. Aman, on the other hand, is young, dynamic, and ambitious. In typical Mumbai fashion, the three are roommates, and the clash of their personalities regularly results in humorous, tongue-in-cheek banter. Things take a turn when the watchman becomes prey to misfortune; a series of chance events results in him stumbling on to a crime. The discovery changes his perspective, boosting his self-confidence enough to make him think that he had a found a new, low-risk way to make money. He then tries to sell the idea to his roommates, to get them to join him in executing a series of such crimes. As they get more and more mired in the spiral of events that follow, the three characters go through several changes as they are pushed more and more against the wall",
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
                "text": [
                    {
                        "category": "Comedy",
                        "confidenceScore": 1.0
                    },
                    {
                        "category": "Drama",
                        "confidenceScore": 1.0
                    }
                ]
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
      "name": "Text Classification",
      "description": "Classify your text",
      "context": "/document",
      "uri": "https://customtextclassifier.azurewebsites.net/api/customtextcla?code=xx==",
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
          "targetName": "class"
        }
      ],
      "httpHeaders": {}
    }
```

## Sample Index Field Definition

The skill will output the text classes that have been extracted for the corpus. In this example, I am expecting several classes so a Collection of ComplexType object is needed, including subfields for category and confidence.

```json
{
  "name": "textclassindex",
  "fields": [
    {
      "name": "id",
      "type": "Edm.String",
      "facetable": false,
      "filterable": false,
      "key": true,
      "retrievable": true,
      "searchable": false,
      "sortable": false,
      "analyzer": null,
      "indexAnalyzer": null,
      "searchAnalyzer": null,
      "synonymMaps": [],
      "fields": []
    },
    {
      "name": "corpus",
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
      "name": "textclass",
      "type": "Collection(Edm.ComplexType)",
      "analyzer": null,
      "synonymMaps": [],
      "fields": [
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
          "facetable": true,
          "filterable": true,
          "retrievable": true,
          "sortable": false,
          "analyzer": null,
          "indexAnalyzer": null,
          "searchAnalyzer": null,
          "synonymMaps": [],
          "fields": []
        }
      ]
    }
}
```

## Sample Indexer Output Field Mapping

The output enrichment of your skill can be directly mapped to one of your fields described above. This can be done with the indexer setting:
```
  "outputFieldMappings": [
    {
      "sourceFieldName": "/document/class",
      "targetFieldName": "textclass"
    }
  ],
```
