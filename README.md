# Azure Search Power Skills

A collection of useful functions to be deployed as custom skills for Azure Cognitive Search .

## Features

This project framework provides the following functions:

* [**GeoPointFromName**](Geo/GeoPointFromName/README.md): retrieves coordinates from place names and addresses.
* [**BingEntitySearch**](Text/BingEntitySearch/README.md): finds rich and structured information about public figures, locations, or organizations.
* [**AcronymLinker**](Text/AcronymLinker/README.md): provides definitions for known acronyms.
* [**ImageStore**](Vision/ImageStore/README.md): stores base64-encoded images into blob storage.

## Getting Started

### Prerequisites

In order to use the functions in this project, you'll need an active Azure subscription. Most of the functions can be used on their own for quick evaluation and experimentation, but they are meant to be used as part of an [Azure Cognitive Search pipeline](https://docs.microsoft.com/en-us/azure/search/cognitive-search-quickstart-blob).
Each function may also add its own specific requirements, such as API keys for services they leverage.

[Visual Studio](https://visualstudio.microsoft.com/) is recommended, but not required. [Postman](https://www.getpostman.com/) is highly recommended as a way to experiment and test skills.

### Installation and deployment

If using Visual Studio with the Azure workload installed, no installation is required, and the functions can just be run locally using F5.

Deployment of a function to Azure can be done [through Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/deployment-zip-push), the Deploy to Azure button, or [continuous deployment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-continuous-deployment).

Some functions may require setting environment variables or configuration entries. Please refer to the readme file in the function's directory.

### Quickstart

1. `git clone git@github.com:Azure-Samples/azure-search-power-skills.git`
2. Open the PowerSkills solution in Visual Studio
3. Set the project for the function to test as the startup project
4. Hit F5
5. Experiment with calling the function using Postman

## Resources

- [Contribution guidelines](CONTRIBUTING.md)
- [Azure Search](https://azure.microsoft.com/en-us/services/search/)
- [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)
- [JFK Files](https://github.com/microsoft/AzureSearch_JFK_Files)
- [Knowledge Mining Solution Accelerator Guide](https://github.com/Azure-Samples/azure-search-knowledge-mining)