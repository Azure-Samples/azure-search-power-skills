---
page_type: sample
languages:
- csharp
products:
- azure
- azure-search
azureDeploy: https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/main/Video/VideoIndexer/azuredeploy.json
name: "Sample skill for enabling video indexing"
description: "This custom skill will invoke the Azure Video Indexer, placing a simplified insights model back into Blob Storage. You can then trigger another indexer to merge the insights back into your main search index."
---

# Video Indexer

This custom skill will invoke the [Azure Video Indexer](https://docs.microsoft.com/en-us/azure/media-services/video-indexer/), placing a simplified insights model back into Blob Storage. You can then trigger another indexer to merge the insights back into your main search index.

## Requirements

This skill requires a Video Indexer account, and key to function.

## Settings

This function requires the following application settings.

| Setting Name | Description |
| ---- | --- |
| MediaIndexer_AccountId | Azure Video Indexer Account Id |
| MediaIndexer_Location | Azure Video Indexer Account location, e.g. trial |
| MediaIndexer_AccountKey | Azure Video Indexer Account key |
| MediaIndexer_StorageConnectionString | Azure Storage Connection string pointing to a blob store to store simplified Video Indexer results |
| MediaIndexer_StorageContainer | Name of the container to place the simplified Video Indexer results |
| MediaIndexer_CallbackFunctionCode | Azure Function code enabling the Video Indexer to invoke the callback function |

To get started with video indexer and to get the necessary AccountId and AccountKey, you can follow this [tutorial](https://docs.microsoft.com/azure/media-services/video-indexer/video-indexer-use-apis).

## Deployment

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FVideo%2FVideoIndexer%2Fazuredeploy.json)

## Video Indexer

### Sample Input:

```json
{
    "values": [
        {
            "recordId": "r1",
            "data":
            {
              "metadata_storage_path": "<SAFE-BASE64-ENCODED-PATH-TO-VIDEO>",
              "metadata_storage_name": "<VIDEO-NAME>"
            }
        }
    ]
}
```

### Sample Output:

VIdeo Indexing is an asynchronous process. When the skill runs the results of the process are not available. This skill outputs a single property ```videoId``` which is the Id of the video inside Video Indexer.
The insights from your video will be placed into another blob container when the Video Indexer has finished. This is described in the next section.

```json
{
    "values": [
        {
            "recordId": "r1",
            "data": {
              "videoId": "A235234"
            },
            "errors": [],
            "warnings": []
        }
    ]
}
```

### Sample Insights Blob Output

When the Video Indexer processer completes it executes a callback function which was supplied to it in the original skill. The callback function retrieves the Video Indexer insights (that describes the video as a time series), and flattens it into a simpler structure that can be easily merged into existing indexes. For more information on the Video Indexer insights see [here](https://api-portal.videoindexer.ai/api-details#api=Operations&operation=Get-Video-Index).

```json
{
    "content": "This is a transcript of the video",
    "keyPhrases": [
        "outdoor",
        "vehicle",
        "land vehicle",
        "car",
        "wheel"
    ],
    "organizations": [],
    "persons": [
      "Fred Fibnar"
    ],
    "locations": [
      "Perth",
      "Australia"
    ],
    "indexedVideoId": "e11cad2313",
    "thumbnailId": "650d162b-e2bd-47d6-a11c-7b36a093fe2d",
    "originalVideoEncodedMetadataPath": "<safe-base64-encoded-path>",
    "originalVideoName": "<original-name>"
}

```

## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "videoIndexer",
    "description": "Video Indexer",
    "uri": "[AzureFunctionEndpointUrl]/api/video-indexer?code=[AzureFunctionDefaultHostKey]",
    "batchSize": 1,
    "context": "/document",
    "inputs": [
      {
        "name": "metadata_storage_path",
        "source": "/document/metadata_storage_path"
      },
      {
        "name": "metadata_storage_name",
        "source": "/document/metadata_storage_name"
      }
    ],
    "outputs": [
    ]
}
```

## Indexing the output of the Video Indexer

Finally, you will need an indexer that indexes the contents of the container where simplified insights are placed. Here's an example of that. Notice how in this example we map the ```originalVideoEncodedMetadataPath``` and ```originalVideoName``` which identify the original video index item. 

> NOTE: By default, this skill will output the video contents to the storage account deployed as part of this ARM template so you may need to create a new data source as well.

| Blob Json Property | Description |
| ---- | ---- |
| originalVideoEncodedMetadataPath | Safe Base 64 encoded path of the original video. This is often used as the primary key in the search index |
| originalVideoName | Name of the original video |
| indexedVideoId | Id of the video in the video indexer. This is used to call REST Apis enabling playback / insight retrieval from the Video Indexer |
| thumbnailId | Id of the summary thumbnail from the video indexer. This can be used to display a sample image on your search results screen |
| content | Contains the full transcript of the video |
| keyPhrases | Array of all keywords, labels, topics, sentiments and emotions from the video |
| keyPhrases | Array of all keywords, labels, topics, sentiments and emotions from the video |
| persons | Array of all identified people from the video |
| locations | Array of all identified locations from the video |

```json
{
  "name": "video-insights-indexer",
  "description": null,
  "dataSourceName": "<data-source-for-simplified-insights-container>",
  "skillsetName": null,
  "targetIndexName": "<original-index-you-want-to-merge-insights-into>",
  "parameters": {
    "configuration": {
      "parsingMode": "json"
    }
  },
  "fieldMappings": [
    {
      "sourceFieldName": "originalVideoEncodedMetadataPath",
      "targetFieldName": "metadata_storage_path",
      "mappingFunction": null
    },
    {
      "sourceFieldName": "originalVideoName",
      "targetFieldName": "metadata_storage_name",
      "mappingFunction": null
    },
    {
      "sourceFieldName": "indexedVideoId",
      "targetFieldName": "indexed_video_id",
      "mappingFunction": null
    },
    {
      "sourceFieldName": "thumbnailId",
      "targetFieldName": "indexed_video_thumbnail_id",
      "mappingFunction": null
    }
  ],
  "outputFieldMappings": []
}

```
