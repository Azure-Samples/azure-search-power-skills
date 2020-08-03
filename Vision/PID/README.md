---
page_type: sample
languages:
- python
products:
- azure
- azure-search
- azure-container-registry
- azure-functions
name: PID 
urlFragment: azure-pid-drawing-sample
description: This custom skill extracts specific product/equipment names from PI&D drawings.
Deploy: Manually [deploy the container image as a Azure function](#deployment)
---

# PID Skill

A [piping and instrumentation diagram (P&ID)](https://en.wikipedia.org/wiki/Piping_and_instrumentation_diagram) is a detailed diagram in the process industry which shows the piping and process equipment together with the instrumentation and control devices. Superordinate to the P&ID is the process flow diagram (PFD) which indicates the more general flow of plant processes and the relationship between major equipment of a plant facility. 

This skill is designed to extract equipment information  from specific instrument symbols in engineering diagrams. The skill uses the X, Y coordinates of text extracted by OCR to generate groupings of text based on proximity, vertical and horizontal separation and alignment. 

For best results, set the normalized images to the higest resolution. You can also edit the parameters within the skill to change the sensitivity of how the tags are grouped. Additional logic is applied product tags to determine tag boundaries and hypheated text. The skill returns two json elements, a tag array and text array.

![diagram](/images/custom_skill_design.png).

## Requirements

This skill requires Docker to build a container that will be deployed as an Azure function.

## Settings

The default configuration of the skill identifies tags or equipment and associated text blocks. Tuning the following parameters allows you to se the sensitivity of grouping of individual text spans into a block.

1. ```maxSegment``` defines the max length of a valid text segment
2. ```leftAlignSensitivity``` defines the sensitivity of the algorithm in matching text blocks that are left aligned
3. ```rightAlignSensitivity``` defines the sensitivity of the algorithm in matching text blocks that are right aligned
4. ```centerAlignSensitivty``` defines the sensitivity of the algorithm in matching text blocks that are center aligned

## Deployment

Follow these steps to build the container and deploy the skill as an Azure Function.


1. Navigate to the `diagramskill` folder and build the docker container ```docker build -t pidskill .```
2. Run the container ```docker run -p 8080:80 -it pidskill:latest```
3. Save the image ```docker commit {container id from previous step} pidskill```
4. Push the image to the container registry ```docker push {containerregistry}.azurecr.io/pidskill```

Once the image is in the container registry, you can now create an Azure function to deploy that image to.

1. In the portal, create a new Azure Function App
2. Select the Docker Container option, provide a valid function name
3. Once the deployment is complete, navigate to the resource, select ```Container settings```
4. Select Azure Container Registry for the Image Source 
5. Select the registry, image and tag
6. Set continuous deployment to On to ensure that the skill is updated when a new image is uploaded
7. Save your changes

Your skill should now be configured and you can now navigate to the Functions menu, select the app and get the function URL.




## Sample Skillset Integration

In order to use this skill in a cognitive search pipeline, you'll need to add a skill definition to your skillset.
Here's a sample skill definition for this example (inputs and outputs should be updated to reflect your particular scenario and skillset environment):

```json
{
    "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
    "name": "PIDSkill", 
    "description": "Extracts tags and text blocks from PID drawings",
    "uri": "[Azure Functions URL]",
    "httpMethod": "POST",
    "timeout": "PT30S",
    "context": "/document/normalized_images/*",
    "batchSize": 1,
    "inputs": [
        {
            "name": "file_data",
            "source": "/document/normalized_images/*"
        },
        {
            "name": "layoutText",
            "source": "/document/normalized_images/*/layoutText"
        }
    ],
    "outputs": [
        {
            "name": "tags",
            "targetName": "tags"
        },
        {
            "name": "textBlocks",
            "targetName": "textBlocks"
        }
    ]
}
```

### Indexer Configuration

To ensure that the skill gets the higest quality image as an input, set the following parameters on the configuration object in the indexer parameters.

```json
"configuration": {
    "dataToExtract": "contentAndMetadata",
    "imageAction": "generateNormalizedImages",
    "allowSkillsetToReadFileData": true,
    "normalizedImageMaxWidth": 4200,
    "normalizedImageMaxHeight": 4200
}
```


