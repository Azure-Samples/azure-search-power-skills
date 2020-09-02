# Azure Search Power Skills

Power Skills are a collection of useful functions to be deployed as custom skills for Azure Cognitive Search. The skills can be used as [templates](Template/HelloWorld/README.md) or starting points for your own custom skills, or they can be deployed and used as they are if they happen to meet your requirements. We also invite you to contribute your own work by submitting a [pull request](https://github.com/Azure-Samples/azure-search-power-skills/compare).

## Up for grabs

Here are a few suggestions of simple contributions to get you started:
* Improve documentation: sample code, better documentation are great ways to improve your understanding of existing code and to help other do the same.
* Configuration: some skills can be configured through [application settings and environment variables](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Vision/AnalyzeForm/AnalyzeForm.cs#L46-L50). Some others still have [hard-coded configuration in the code](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Text/CustomEntitySearch/CustomEntityLookup.cs#L28-L31), that could be moved to be easier to configure.
* For skills that rely on an external Azure resource (such as [Bing Entity Search](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Text/BingEntitySearch/BingEntitySearch.cs#L20)), improve the [deployment file](https://github.com/Azure-Samples/azure-search-power-skills/blob/master/Text/BingEntitySearch/azuredeploy.json) so it gives the user the option to create and configure that service automatically.

## Features

This project provides the following custom skills:

* [**HelloWorld**](Template/HelloWorld/README.md): a minimal skill that can be used as a starting point or template for your own skills.
* [**GeoPointFromName**](Geo/GeoPointFromName/README.md): retrieves coordinates from place names and addresses.
* [**BingEntitySearch**](Text/BingEntitySearch/README.md): finds rich and structured information about public figures, locations, or organizations.
* [**AcronymLinker**](Text/AcronymLinker/README.md): provides definitions for known acronyms.
* [**ImageStore**](Vision/ImageStore/README.md): stores and fetches base64-encoded images to and from blob storage.
* [**HocrGenerator**](Vision/HocrGenerator/README.md): transforms the result of OCR into the hOCR format.
* [**AnalyzeForm**](Vision/AnalyzeForm/README.md): recognizes form fields in a document.
* [**CustomVision**](Vision/CustomVision/README.md): classifies documents using [Custom Vision](https://customvision.ai) models.
* [**CustomEntityLookup**](/Text/CustomEntitySearch): finds custom entity names in text.
* [**Tokenizer**](Text/Tokenizer/README.md): extracts non-stop words from a text.
* [**Distinct**](Text/Distinct/README.md): de-duplicates a list of terms.
* [**P&ID Parser**](Vision/PID/README.md): Extracts equipment tags and text blocks from piping and instrumentation diagrams
* [**GetFileExtension**](Utils/GetFileExtension/README.md): returns the filename and extension as separate values allowing you to filter on document type.
* [**DecryptBlobFile**](Utils/DecryptBlobFile/README.md): downloads, decrypts and returns a file that was previously encrypted and stored in Azure Blob Storage.

## Getting Started

### Prerequisites

In order to use the functions in this project, you'll need an active Azure subscription. Most of the functions can be used on their own for quick evaluation and experimentation, but they are meant to be used as part of an [Azure Cognitive Search pipeline](https://docs.microsoft.com/azure/search/cognitive-search-quickstart-blob).
Each function may also add its own specific requirements, such as API keys for services they leverage.

[Visual Studio 2019](https://visualstudio.microsoft.com/) is recommended, but not required. You need a recent version of the C# compiler. [Postman](https://www.getpostman.com/) is highly recommended as a way to experiment and test skills.

### Installation and deployment

If using Visual Studio with the Azure workload installed, no installation is required, and the functions can just be run locally using F5.

Deployment of a function to Azure can be done [through Visual Studio](https://docs.microsoft.com/azure/azure-functions/deployment-zip-push), the Deploy to Azure button, or [continuous deployment](https://docs.microsoft.com/azure/azure-functions/functions-continuous-deployment).

Some functions may require setting environment variables or configuration entries. Please refer to the readme file in the function's directory.

### Quickstart

1. Clone the repository
2. Open the PowerSkills solution in Visual Studio
3. Set the project for the function to test as the startup project
4. Hit F5
5. Experiment with calling the function using Postman

You can also create your own skills using [our Hello World template skill](Template/HelloWorld/README.md) as a starting point.

## Resources

- [Contribution guidelines](CONTRIBUTING.md)
- [Azure Search](https://azure.microsoft.com/services/search/)
- [Azure Functions](https://azure.microsoft.com/services/functions/)
- [JFK Files](https://github.com/microsoft/AzureSearch_JFK_Files)
- [Knowledge Mining Solution Accelerator Guide](https://github.com/Azure-Samples/azure-search-knowledge-mining)
