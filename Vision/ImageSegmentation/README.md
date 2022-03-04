# image_segment_skill
---
Description:
- Quite often we need to break down a full page image (or PDF page) into the subimages included inside. 
Within the Knowledge Mining ecosystem it helps greatly to discover assets in a more granular way, for example a technical document full of subimages that include OCRed text.
Decomposing the page into several subimages help to render a more friendly UI and even further process those images (ie to classify a subimage into a fossile, a rock, etc)
 
- image_segment_skill is an Azure Cognitive Search skill to break down one big image into its internal subimages, all within a Azure Cognitive Search skillset. 

Languages:
- ![python](https://img.shields.io/badge/language-python-orange)

Products:
- Azure Cognitive Search
- Azure Functions
---

# Settings
This function requires two appsettings to save the extracted images on an Azure Blob Storage. -RECOMMENDED-

If you want to save a base64 encoded image version in the Azure Cognitive Search index the settings are not required.

```json
    "blob_storage_connection_string": "DefaultEndpointsProtocol=https;AccountName=YOUR_BLOB_ACCOUNT_NAME;AccountKey=YOUR_BLOB_ACCOUNT_KEY;EndpointSuffix=core.windows.net",
    "blob_storage_container": "image-segmentation-skillset-image-projection" // OR ANY OTHER CONTAINER NAME
```
# Steps    

1. Create a Python Function in Azure, for example this is a good [starting point](https://docs.microsoft.com/azure/azure-functions/create-first-function-vs-code-python)
2. Clone this repository
3. Open the folder in VS Code and deploy the function, find here a [tutorial](https://docs.microsoft.com/azure/search/cognitive-search-custom-skill-python)
4. Add a field in your index where you will dump the enriched classes, more info [here](#sample-index-field-definition)
5. Add the skill to your skillset as [described below](#sample-skillset-integration)
6. Add the output field mapping in your indexer as [seen in the sample](#sample-indexer-output-field-mapping)
7. Run the indexer 

## Sample Input:

You can find a sample input for the skill [here](../main/custom_ner/sample.dat)

```json
{
    "values": [
        {
            "recordId": "e1",
            "data": {
                "images": [
                    {
                        "$type": "file",
                        "url": "optional",
                        "data": "/9j/4AAQSkZx... ",
                        "width": 1224,
                        "height": 1584,
                        "originalWidth": 1224,
                        "originalHeight": 1584,
                        "rotationFromOriginal": 0,
                        "contentOffset": 4132,
                        "pageNumber": 2,
                        "contentType": "image/jpeg"
                    }
                ]
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
            "recordId": "e1",
            "data": {
                "normalized_images_merged": [
                    {
                        "$type": "file",
                        "contentType": "image/jpeg",
                        "data": "/9j/4A...",
                        "height": 128,
                        "width": 187,
                        "pageNumber": 2,
                        "image_url" : "https:// (optional. only if defined app settings)"
                    },
                    {
                        "$type": "file",
                        "contentType": "image/jpeg",
                        "data": "/9j/4A...",
                        "height": 128,
                        "width": 187,
                        "pageNumber": 2,
                        "image_url" : "https:// (optional. only if defined app settings)"
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
      "name": "#x",
      "description": "Custom skill for image segmentation",
      "context": "/document",
      "uri": "https://my-func.azurewebsites.net/api/imagesegment?code= ",
      "httpMethod": "POST",
      "timeout": "PT30S",
      "batchSize": 1,
      "degreeOfParallelism": null,
      "inputs": [
        {
          "name": "images",
          "source": "/document/normalized_images/*"
        }
      ],
      "outputs": [
        {
          "name": "normalized_images_merged",
          "targetName": "subimages"
        }
      ],
      "httpHeaders": {}
    },
```

## Sample Index Field Definition

The skill emits an array of base64 images extracted for the pages.

The skill emits also an array of image urls if you do not want to store the base64 image within the index.

In both cases, other subfields like image height, etc can be persisted too

```json
    {
      "name": "subimageb64",
      "type": "Collection(Edm.String)",
      "facetable": false,
      "filterable": false,
      "retrievable": true,
      "searchable": false,
      "analyzer": null,
      "indexAnalyzer": null,
      "searchAnalyzer": null,
      "synonymMaps": [],
      "fields": []
    }
    {
      "name": "subimageurls",
      "type": "Collection(Edm.String)",
      "facetable": false,
      "filterable": false,
      "retrievable": true,
      "searchable": false,
      "analyzer": null,
      "indexAnalyzer": null,
      "searchAnalyzer": null,
      "synonymMaps": [],
      "fields": []
    }
```
## Sample Indexer 

Note that the function receives a base64 encoded representation of the image, processes it and returns a set of base64 subimages and image urls pointing to an Azure Blob Storage (only if Azure Function app settings are correctly configured)

```
  "outputFieldMappings": [
    {
      "sourceFieldName": "/document/merged_content",
      "targetFieldName": "merged_content"
    },
...
    {
      "sourceFieldName": "/document/subimages/*/data",
      "targetFieldName": "subimageb64"
    }
    ...
    {
      "sourceFieldName": "/document/subimages/*/image_data",
      "targetFieldName": "subimageurls"
    }
```
    
