![python](https://img.shields.io/badge/language-python-orange)
![C#](https://img.shields.io/badge/language-C%23-brightgreen)

> [!IMPORTANT]  
> As of November 15, 2023, Azure Cognitive Search has been renamed to Azure AI Search.

# Azure AI Search Custom Skills Samples (AI enrichment skillsets)

A collection of sample implementations for **custom skill functionality in Azure AI Search**.

> **Previously known as:** **Azure Search Power Skills**  
> This repository may still be referenced by its former name in older documentation, links, and conversations.

> **Terminology clarification**  
> In this repository, “skills” refers to **Azure AI Search skillset enrichment skills** (including **Custom Web API skills** used during indexing), not agent/copilot skills.

## Why this repository exists

Azure AI Search includes many built-in enrichment skills.  
This repository provides examples for scenarios where you need **custom logic** that built-in skills don’t provide directly, such as:

- Custom transformations and enrichment output shaping
- Integration with external services or proprietary systems
- Deterministic pre/post-processing in indexing pipelines

## What you’ll find

This repo contains reusable samples and patterns for:

- Building and hosting **Custom Web API skills**
- Implementing custom enrichment components in C# and Python
- Testing and iterating on enrichment logic
- Integrating custom outputs into Azure AI Search skillsets

Language composition of this repository:
- C# (~70%)
- Jupyter Notebook (~17%)
- Python (~11%)

## When to use built-in skills vs custom skills

Use **built-in skills** when:
- The required enrichment already exists in Azure AI Search
- You can achieve the needed output with standard configuration

Use **custom skills** when:
- You need enrichment behavior not available out-of-the-box
- You need strict control over transformation/business logic
- You must call external APIs, private models, or internal systems

## Core Azure AI Search documentation

Start here for official guidance:

- **Custom Web API skill**  
  https://learn.microsoft.com/azure/search/cognitive-search-custom-skill-web-api

- **Create and define skillsets**  
  https://learn.microsoft.com/azure/search/cognitive-search-defining-skillset

- **Built-in skills reference**  
  https://learn.microsoft.com/azure/search/cognitive-search-predefined-skills

- **Indexers in Azure AI Search**  
  https://learn.microsoft.com/azure/search/search-indexer-overview

- **Enrichment concepts (skillsets/pipeline context)**  
  https://learn.microsoft.com/azure/search/cognitive-search-concept-intro

## How to use these samples

1. Identify the enrichment gap in your indexing pipeline.
2. Select the closest sample in this repository.
3. Adapt the custom logic for your data/contracts.
4. Host the skill endpoint (for Web API-based skills).
5. Add/update your skillset to call the custom skill.
6. Run and validate indexer execution and output.

## Scope note

This repository is specifically for **Azure AI Search skillset custom enrichment samples** and related implementation patterns.

## Skills

This project provides the following custom skills:


| Skill | Description | Type |Language | Environment |Deployment|
| --- | ----------- | -------| ----------- | ----------- | ----------- |
| [Chat completion Custom Skill](ChatCompletionCustomSkill/README.md) | Demonstrates how to use chat-completion inference capabilities of language models (including Azure OpenAI models) deployed in Azure AI Foundry in a custom web API skillset in Azure AI Search. | Text | ![python](https://img.shields.io/badge/language-python-orange) | Azure Functions | [![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://ms.portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FChatCompletionCustomSkill%2Fazuredeploy.json) |
| [GeoPointFromName](Geo/GeoPointFromName/README.md) | Retrieves coordinates from place names and addresses. | Geography | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | [![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://ms.portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmaster%2FGeo%2FGeoPointFromName%2Fazuredeploy.json) |
| [Distinct](Text/Distinct/README.md) | De-duplicates a list of terms. | Text | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | |
| [TextAnalyticsForHealth](Text/TextAnalyticsForHealth/README.md) | A wrapper for the Text Analytics for Health API. | Text | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | [![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://ms.portal.azure.com/#create/Microsoft.Template/uri/https://raw.githubusercontent.com/Azure-Samples/azure-search-power-skills/main/Text/TextAnalyticsForHealth/azuredeploy.json) |
| [TextQualityWatchdog](Text/TextQualityWatchdog/README.md) | Uses a pretrained language model to detect low-quality text extracted during document cracking. | Text | ![python](https://img.shields.io/badge/language-python-orange) | Azure Function and Azure Open AI endpoint | |
| [DecryptBlobFile](Utils/DecryptBlobFile/README.md) | Downloads, decrypts, and returns a file previously encrypted and stored in Azure Blob Storage. | Utility | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | |
| [GetFileExtension](Utils/GetFileExtension/README.md) | Returns the filename and extension as separate values to allow filtering on document type. | Utility | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | |
| [HelloWorld](Template/HelloWorld/README.md) | A minimal skill that can be used as a starting point or template for your own skills. | Template | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | [![Deploy to Azure](http://azuredeploy.net/deploybutton.svg)](https://ms.portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fazure-search-power-skills%2Fmain%2FTemplate%2FHelloWorld%2Fazuredeploy.json) |
| [PythonFastAPI](Template/PythonFastAPI/README.md) | A production web server and API scaffold for a Python power skill. | Template | ![python](https://img.shields.io/badge/language-python-orange) | Azure Functions | |
| [VideoIndexer](Video/VideoIndexer/README.md) | Extracts insights from video content for indexing workflows. | Vision | ![C#](https://img.shields.io/badge/language-C%23-brightgreen) | Azure Functions | |



## Getting Started

### Prerequisites

In order to use the functions in this project, you'll need an active Azure subscription. Most of the functions can be used on their own for quick evaluation and experimentation, but they are meant for integration in Azure AI Search enrichment pipelines.  
Each function may also add its own specific requirements, such as API keys for services they leverage.

[Visual Studio](https://visualstudio.microsoft.com/) is recommended, but not required. You need a recent version of the C# compiler.

### Installation and deployment

If using Visual Studio with the Azure workload installed, no installation is required, and the functions can just be run locally using F5.

Deployment of a function to Azure can be done [through Visual Studio](https://learn.microsoft.com/azure/azure-functions/deployment-zip-push), the Deploy to Azure button, or [continuous deployment](https://learn.microsoft.com/azure/azure-functions/functions-continuous-deployment) from source control.

Some functions may require setting environment variables or configuration entries. Please refer to the readme file in the function's directory.

### Quickstart

1. Clone the repository
2. Open the PowerSkills solution in Visual Studio
3. Set the project for the function to test as the startup project
4. Hit F5
5. Experiment with calling the function using your preferred HTTP client or test harness

You can also create your own skills using [our Hello World template skill](Template/HelloWorld/README.md) as a starting point 
or if you are using python [our FastAPI template skill](Template/PythonFastAPI/README.md).

## Up for grabs

Here are a few suggestions of simple contributions to get you started:
* Improve documentation: sample code, better documentation are great ways to improve your understanding of existing code and to help others do the same.
* Configuration: some skills can be configured through application settings and environment variables. You can use existing skills in this repo (for example, Azure Function-based samples under `Text/`, `Utils/`, `Template/`) as references for clearer setup guidance and consistent config naming.
* For skills that rely on external Azure resources or APIs (for example, skills under `Video/VideoIndexer` or `Text/TextAnalyticsForHealth`), consider adding deployment templates and setup instructions to simplify onboarding and reproducibility.

## Resources

- [Contribution guidelines](CONTRIBUTING.md)
- [Azure AI Search documentation](https://learn.microsoft.com/azure/search/)
- [Custom Web API skill (official docs)](https://learn.microsoft.com/azure/search/cognitive-search-custom-skill-web-api)
- [Azure Functions documentation](https://learn.microsoft.com/azure/azure-functions/)
- [JFK Files sample dataset](https://github.com/microsoft/AzureSearch_JFK_Files)
- [Knowledge Mining Solution Accelerator Guide](https://github.com/Azure-Samples/azure-search-knowledge-mining)
