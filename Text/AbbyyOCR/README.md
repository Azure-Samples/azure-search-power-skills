---
topic: sample
languages:
- csharp
products:
- azure-cognitive-search
- ABBYY OCR
name: ABBYY OCR Custom Skill for Azure Cognitive Search
description: This custom skill leverages [ABBYY Cloud OCR](https://www.ocrsdk.com/) to extract text from images  and finds user defined entities in given texts.
---

# ABBYY OCR Custom Skill for Azure Cognitive Search

This is a custom skill for Azure Cognitive Search that leverages [ABBYY Cloud OCR](https://www.ocrsdk.com/) to extract text from images.  It is code that leverages Azure Functions to receive input from Azure Cognitive Search to take an image which is passed to ABBYY OCR and returns text back to Azure Cognitive Search.

## Requirements
* Azure Cognitive Search Service: Please ensure you know how to setup and configure an [Azure Cognitive Search web api based custom skill](https://docs.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api)
* ABBYY Cloud OCR SDK: Register for [ABBYY Cloud OCR SDK](https://www.ocrsdk.com/) account, which comes with 500 free calls.  Once registered, you will need to create an OCR application and ensure you have an Application ID, Password, and ServiceUrl such as https://cloud-westus.ocrsdk.com.
* Postman:  We will use [Postman](https://www.postman.com/downloads/) to test the Custom Skill
* Azure Cognitive Search Power Skills: Download or Clone the [Power Skills](https://github.com/Azure-Samples/azure-search-power-skills) repo 

# Configuration

Go to the location where you downloaded the Azure Cognitive Search Power Skills and find the Text directory.  Within this directory, copy the ABBYYOCR directory from this repo.

Open PowerSkills.sln in the root directory of this Power Skills directory in Visual Studio.  

In the Solution Explorer, Right Click on Text and Choose Add -> Existing Project.

Locate and choose AbbyyOCR.csproj within the \Text\ABBYOCR directory and add it.  

Open AbbyyOCR.cs and set the environment variables for the following lines:

```csharp
        private static readonly string ApplicationId = @"[Enter ABBYY Application ID]";
		private static readonly string Password = @"[Enter ABBYY OCR Password]";
		private static readonly string ServiceUrl = "[Enter ABBYY Service URL such as https://cloud-westus.ocrsdk.com]";

```

This code assumes that the images will be coming in, in either English, Arabic or Hebrew.  If you wish to change this, update the below code in the ProcessImageAsync function. You can find the list of [supported languages here](https://www.ocrsdk.com/documentation/specifications/recognition-languages/).

```code
var imageParams = new ImageProcessingParams
{
     ExportFormats = new[] { ExportFormat.Docx, ExportFormat.Txt, },
     Language = "English,Arabic,Hebrew",
};
```

# Test

At this point we can test this function.  Click on ABBYYOCR in the Solution Explorer and choose F5.  Once it is running you will see a URL such as http://localhost:7071/api/AbbyyOCR.

Open Postman and create a POST request to this URL.

In the Body of the request, change the type to raw and enter JSON in a format such as this the below JSON.

NOTE: For your test, you will need to not only have the URL references to the Blob image which is placed in the formURL field, but also create a SaS Token for this file which is located in the formSasToken field.

```json
{
  "values": 
  [
     {
      "recordId": "foo1",
      "data": { 
             "formUrl":  "https://azsdemos.blob.core.windows.net/blob-files/image1.JPG",
             "formSasToken":  "?sv=..."
      }
    },
         {
      "recordId": "foo2",
      "data": { 
             "formUrl":  "https://azsdemos.blob.core.windows.net/blob-files/image2.JPG",
             "formSasToken":  "?sv=..."
      }
    }
  ]
}
```

The output will look similar to this:

```json
{
    "values": [
        {
            "recordId": "foo2",
            "data": {
                "content": "التنمر..."
            },
            "errors": [],
            "warnings": []
        },
        {
        }
    ]
}
```

# Deploy

At this point you can deploy this code to Azure Functions by right clicking on the ABBYYOCR project in the Solution Explorer and choosing Publish. Deploy this to Azure Functions (App Services).

Once deployed, test again.   You will likely need to open this function in the Azure Portal to get the full URL which will include a code for authentication such as https://my-test-abbyy.azurewebsites.net/api/AbbyyOCR?code=/gECNMreXdczJhGZKhziNiMnABC123==

Test this once again in Postman before deploying to Azure Cognitive Search.

# Example Skillset

NOTE: You may wish to adjust the degree of parallelism (degreeOfParallelism) up to 10 and will want to update [SET_CODE] with our ABBYY OCR code.

```json
{
    "name": "abbyy-ocr-skillset",
    "description": "Skillset created from the portal. skillsetName: abbyy-ocr-skillset;",
    "skills": [
        {
            "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
            "name": "#1",
            "description": "Convert Image to Text",
            "context": "/document",
            "uri": "https://my-abbyy-ocr.azurewebsites.net/api/AbbyyOCR?code=/[SET_CODE]",
            "httpMethod": "POST",
            "timeout": "PT30S",
            "batchSize": 4,
            "degreeOfParallelism": 5,
            "inputs": [
                {
                    "name": "formUrl",
                    "source": "/document/storage_url_decoded",
                    "sourceContext": null,
                    "inputs": []
                },
                {
                    "name": "formSasToken",
                    "source": "/document/metadata_storage_sas_token",
                    "sourceContext": null,
                    "inputs": []
                }
            ],
            "outputs": [
                {
                    "name": "content",
                    "targetName": "ocr_content"
                }
            ],
            "httpHeaders": {}
        }
    ],
    "cognitiveServices": {
        "@odata.type": "#Microsoft.Azure.Search.CognitiveServicesByKey",
        "description": "",
        "key": "[Enter Your Cog Service Key]"
    }
}
```

# Example Search Index Schema
```json
{
    "name": "abbyy-ocr",
    "defaultScoringProfile": "",
    "fields": [
        {
            "name": "content",
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
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_content_type",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": false,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_size",
            "type": "Edm.Int64",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": false,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_last_modified",
            "type": "Edm.DateTimeOffset",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": true,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_content_md5",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_name",
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
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_path",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "storage_url_encoded",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": true,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "storage_url_decoded",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_storage_file_extension",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": false,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "metadata_content_type",
            "type": "Edm.String",
            "searchable": false,
            "filterable": true,
            "retrievable": true,
            "sortable": false,
            "facetable": true,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        },
        {
            "name": "ocr_content",
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
            "synonymMaps": []
        },
        {
            "name": "enriched",
            "type": "Edm.String",
            "searchable": false,
            "filterable": false,
            "retrievable": true,
            "sortable": false,
            "facetable": false,
            "key": false,
            "indexAnalyzer": null,
            "searchAnalyzer": null,
            "analyzer": null,
            "synonymMaps": []
        }
    ],
    "scoringProfiles": [],
    "corsOptions": null,
    "suggesters": [
        {
            "name": "sg",
            "searchMode": "analyzingInfixMatching",
            "sourceFields": [
                "metadata_storage_name"
            ]
        }
    ],
    "analyzers": [],
    "tokenizers": [],
    "tokenFilters": [],
    "charFilters": [],
    "encryptionKey": null
}
```

# Example Indexer
```json
{
    "name": "abbyy-ocr-indexer",
    "description": "",
    "dataSourceName": "abbyy-ocr",
    "skillsetName": "abbyy-ocr-skillset",
    "targetIndexName": "abbyy-ocr",
    "disabled": null,
    "schedule": null,
    "parameters": {
        "batchSize": null,
        "maxFailedItems": 0,
        "maxFailedItemsPerBatch": 0,
        "base64EncodeKeys": false,
        "configuration": {
            "dataToExtract": "allMetadata",
            "parsingMode": "default",
            "imageAction": "generateNormalizedImages"
        }
    },
    "fieldMappings": [
        {
            "sourceFieldName": "metadata_storage_path",
            "targetFieldName": "metadata_storage_path",
            "mappingFunction": {
                "name": "urlEncode",
                "parameters": null
            }
        },
        {
            "sourceFieldName": "metadata_storage_path",
            "targetFieldName": "storage_url_encoded",
            "mappingFunction": {
                "name": "base64Encode",
                "parameters": null
            }
        },
        {
            "sourceFieldName": "metadata_storage_path",
            "targetFieldName": "storage_url_decoded",
            "mappingFunction": null
        }
    ],
    "outputFieldMappings": []
}
```

# Example Data Source
```json
{
    "name": "abbyy-ocr",
    "description": null,
    "type": "azureblob",
    "subtype": null,
    "credentials": {
        "connectionString": null
    },
    "container": {
        "name": "images-ocr",
        "query": null
    },
    "dataChangeDetectionPolicy": null,
    "dataDeletionDetectionPolicy": null
}
```

