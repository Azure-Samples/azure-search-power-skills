---
page_type: sample
languages:
  - python
products:
  - azure
  - azure-cognitive-search
  - azure-ai-vision
name: Image Analysis v4.0 Sample Skill for AI Search (Flex Consumption Plan)
description: "This custom skill calls the Azure AI Vision v4.0 API to perform OCR and optionally generate captions for images within an Azure AI Search pipeline. Deploys to the Azure Functions Flex Consumption plan."
# Deploy button for infrastructure
azureDeploy: https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FVision%2FImageAnalysisV4%2Fazuredeploy.json
---

# Image Analysis v4.0 Skill (Flex Consumption Plan)

This custom skill integrates Azure AI Vision v4.0 image analysis capabilities into an Azure AI Search enrichment pipeline. It processes images provided by the indexer, performs Optical Character Recognition (OCR) to extract text, and can optionally generate image captions.

This version utilizes the **Azure Functions Flex Consumption plan** and the function's **System-Assigned Managed Identity** for secure connections.

## Requirements

- In addition to the common requirements described in [the root `README.md` file](../../README.md):
  - This skill requires access to an [Azure AI Vision resource](https://learn.microsoft.com/azure/ai-services/computer-vision/overview-image-analysis).
  - **Manual Action Required:** The Function App's Managed Identity (created during deployment) must be granted the **`Cognitive Services User`** role on the target Azure AI Vision resource post-deployment. See [Assign Azure roles using the Azure portal](https://learn.microsoft.com/azure/role-based-access-control/role-assignments-portal) or use Azure CLI. You can get the Function App's Principal ID from the ARM deployment output (`functionAppPrincipalId`).

## Settings

This function requires the following Application Setting to be configured in the deployed Azure Function App **after** infrastructure deployment:

- `AI_VISION_ENDPOINT`: The endpoint URL of your Azure AI Vision resource (e.g., `https://your-vision-resource-name.cognitiveservices.azure.com/`).

## Deployment

**This skill requires a two-step deployment process due to the Flex Consumption plan:**

1. **Deploy Infrastructure:** Click the button below to deploy the Azure resources (Flex Plan, Function App structure, Storage, Application Insights, Managed Identity configuration). Note the `functionAppName` and `functionAppPrincipalId` outputs.
   [![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FVision%2FImageAnalysisV4%2Fazuredeploy.json)

2. **Deploy Function Code:** After the infrastructure deployment succeeds, deploy the Python code using [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Cazure-cli%2Cportal%2Cbash#install-the-azure-functions-core-tools) (v4):

   - Install Core Tools if you haven't already.
   - Navigate (`cd`) to this skill's directory (`Vision/ImageAnalysisV4`) in your local clone of the repository.
   - Run the following command, replacing `<YourFunctionAppName>` with the name outputted by the ARM deployment:

     ```bash
     func azure functionapp publish <YourFunctionAppName> --python
     ```

3. **Configure Settings & Permissions:** Complete the **manual actions** described in the [Requirements](#requirements) and [Settings](#settings) sections (assign `Cognitive Services User` role and set the `AI_VISION_ENDPOINT` application setting).

## Required Post-Deployment Steps

**1. Configure Application Settings:**

You **must** configure the endpoint for your AI Vision resource.

- Navigate to the deployed Function App in the Azure Portal.
- Go to **Settings -> Configuration -> Application settings**.
- Find the setting named `AI_VISION_ENDPOINT`.
- Click on it, set its **Value** to the endpoint URL of your Azure AI Vision resource (e.g., `https://your-vision-resource-name.cognitiveservices.azure.com/`). **Ensure the URL starts with `https://`**.
- Click **OK** and then **Save** the application settings. (This will restart the Function App).

**Alternatively, using Azure CLI:**

```bash
# Get the Function App name from the ARM deployment output
functionAppName="<YourFunctionAppName>"
resourceGroupName="<YourResourceGroupName>" # The group you deployed to
visionEndpoint="<YourVisionResourceEndpoint>" # e.g., https://your-vision-resource-name.cognitiveservices.azure.com/

az functionapp config appsettings set \
    --resource-group "$resourceGroupName" \
    --name "$functionAppName" \
    --settings AI_VISION_ENDPOINT="$visionEndpoint"
```

**2. Grant Permissions to AI Vision Resource:**

The Function App uses its Managed Identity to authenticate securely with the AI Vision service. You need to grant this identity permission to call the Vision API.

- **Get the Function App's Principal ID:** Provided as an output (`functionAppPrincipalId`) from the ARM deployment (Step 1).
- **Assign the Role:** Grant the **`Cognitive Services User`** role to this Principal ID on the **scope** of your AI Vision resource.
  - _(Instructions for Portal/CLI remain the same)_

## Sample Input:

```json
{
  "values": [
    {
      "recordId": "rec1",
      "data": {
        "image": "/9j/4AAQSkZJRgABAQEAAAAAAAD/4...", // Base64 encoded image string
        "languageCode": "en" // Optional: Provided by Language Detection skill or set statically
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
      "recordId": "rec1",
      "data": {
        "image_text": "Text extracted from the image.",
        "caption": "A descriptive caption if requested." // Included if use_caption=true
      },
      "errors": null,
      "warnings": null
    }
  ]
}
```

_(See function code for detailed error/warning structure)_

## Sample Skillset Integration

To use this skill in an AI Search pipeline, add a skill definition to your skillset.

**Important:**

1. Update `[Your Function Endpoint]` with the URL of your deployed Azure Function App (e.g., `https://<YourFunctionAppName>.azurewebsites.net`).
2. Update `[Your Function Key]` with a function key (**Host key** recommended) for your deployed app. Get keys from the Portal (Function App -> App keys) or CLI (`az functionapp keys list ...`).
3. Set `use_caption=true` in the uri query string if you want captions.

```json
{
  "@odata.type": "#Microsoft.Skills.Custom.WebApiSkill",
  "name": "ImageAnalysisV4Skill",
  "description": "Performs OCR and optionally generates captions using Azure AI Vision v4.0 (Flex Plan)",
  "context": "/document/normalized_images/*",
  // Example URI requesting captions:
  "uri": "[Your Function Endpoint]/api/aivisionapiv4?code=[Your Function Key]&use_caption=true",
  "httpMethod": "POST",
  "timeout": "PT230S", // Max timeout recommended
  "batchSize": 1, // Start low
  "degreeOfParallelism": 1, // Start low
  "inputs": [
    { "name": "image", "source": "/document/normalized_images/*/data" },
    { "name": "languageCode", "source": "/document/languageCode" } // Assumes LanguageDetectionSkill ran before
  ],
  "outputs": [
    { "name": "image_text", "targetName": "extractedImageText" }, // Example target name
    { "name": "caption", "targetName": "generatedCaption" } // Example target name
  ]
}
```

## Troubleshooting

- **HTTP 500 Error with `Bearer token authentication is not permitted for non-TLS protected (non-https) URLs`:** This usually means the `AI_VISION_ENDPOINT` application setting is missing, empty, or does not start with `https://`. Verify the setting in the Function App configuration, ensure it includes `https://`, and **Save** the configuration to restart the app.
- **HTTP 500 Error with `(PermissionDenied) Principal does not have access to API/Operation`:** This means the Function App's Managed Identity (Principal ID: `<outputted principal id>`) has not been granted the **`Cognitive Services User`** role on the target AI Vision resource. Follow step 2 in the "Required Post-Deployment Steps" above.
- **Function not appearing after `func publish`:** Check the output of the publish command for errors. Ensure the deployment storage container exists and the Function App has the correct role assignments on the storage account (handled by the ARM template). Check Application Insights logs for startup errors.
