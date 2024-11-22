# Azure Open AI Custom Inference Skill

---

Quickstart Guide:

- This folder illustrates how to leverage Azure AI Studio to summarize a large piece of text using [custom web api skills](https://learn.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api).

- As a prerequisite to running this custom skill, you must first deploy a model to Azure. See [this guide](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-openai) for reference.

- in the *local.settings.json* file, set your "AZURE_INFERENCE_CREDENTIAL" value to the API key of your deployed model. Set the "AZURE_CHAT_COMPLETION_ENDPOINT" environment variable to the url where your model is hosted

- Once you are in this folder, you need to install the Azure functions Visual Studio extension and update the core tools. After that, you should run func start as descibed in this [python quickstart for Azure functions](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-python?tabs=windows%2Cbash%2Cazure-cli%2Cbrowser).

- A sample payload is provided in the *api-test.http* file. Replace the localhostBaseUrl variable with the base url of your container/local http environment. Once you install the Visual Studio REST Client extension, you can hit the Send Request button from that file.

- The currently demonstrated set of custom skills in this repository are about using chat completion models to do entity recognition, summarization and image-captioning.

- When it comes time to deploy your code in Azure, you can follow [this guide for setting up your Python function](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-python?tabs=windows%2Cbash%2Cazure-cli%2Cbrowser#create-supporting-azure-resources-for-your-function)

Languages:

- ![python](https://img.shields.io/badge/language-python-orange)

Products:

- Azure AI Studio
- Azure Functions