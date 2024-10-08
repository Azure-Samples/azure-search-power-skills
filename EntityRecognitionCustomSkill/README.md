# Entity Recognition Summarization Skill

---

Quickstart Guide:

- This folder illustrates how to leverage Azure AI Studio to recognize entities from some text using [custom web api skills](https://learn.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api).

- As a prerequisite to running this custom skill, you must first deploy a model that has chat completion as an inference task into Azure AI Studio. See [this guide](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-openai) for reference.

- in the *local.settings.json* file, set your "AZURE_INFERENCE_CREDENTIAL" value to the API key of your deployed LLM.

- Once you are in this folder, you need to install the Azure functions Visual Studio extension and update the core tools. After that, you should run func start as descibed in this [python quickstart for Azure functions](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-python?tabs=windows%2Cbash%2Cazure-cli%2Cbrowser).

- A sample payload is provided in the *api-test.http* file. Replace the localhostBaseUrl variable with the base url of your container/local http environment. Once you install the Visual Studio REST Client extension, you can hit the Send Request button from that file.

Languages:

- ![python](https://img.shields.io/badge/language-python-orange)

Products:

- Azure AI Studio
- Azure Functions