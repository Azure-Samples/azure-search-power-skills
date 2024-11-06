# Azure Open AI Custom Inference Skill

Welcome to the Azure Open AI Custom Inference Skill repository! This guide helps you understand how to leverage Azure OpenAI services using Azure Functions to create a custom inference skill that performs tasks like summarization, entity recognition, and image captioning.

## 🚀 Quickstart Guide

### Prerequisites

- An **Azure OpenAI Deployed Model**, either `gpt-4o` or `gpt-4o-mini`. You can follow [this guide](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-openai) to deploy your model.

### Setup Steps

1. **Clone the Repository**
   - Clone this repository to your local environment to get started.

2. **Configure Environment Variables**
   - In the `local.settings.json` file, set the following:
     - `AZURE_INFERENCE_CREDENTIAL`: Set this value to the API key of your deployed model.
     - `AZURE_CHAT_COMPLETION_ENDPOINT`: Set this to the URL where your model is hosted.

3. **Install Azure Functions Core Tools**
   - To run this function locally, install the Azure Functions Visual Studio extension and update the core tools. Follow the [Azure Functions Python Quickstart Guide](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-python?tabs=windows%2Cbash%2Cazure-cli%2Cbrowser) to set up your environment.

4. **Start the Azure Function**
   - Navigate to the folder and run `func start` to initiate the function locally.

5. **Test the API Locally**
   - Use the `api-test.http` file to test the function locally. Replace the `localHostBaseUrl` variable with the base URL of your local environment. Install the Visual Studio REST Client extension to make testing easier by clicking the **Send Request** button.

### 🛠️ Current Custom Skills
- **Summarization**: Generates a concise summary from the input text.
- **Entity Recognition**: Identifies entities such as people, places, and organizations from the input text.
- **Image Captioning**: Provides a descriptive caption for an image provided either via a URL or as base64-encoded data.

### 🖥️ Running in Azure
When ready to deploy, you can follow [this guide](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-python?tabs=windows%2Cbash%2Cazure-cli%2Cbrowser#create-supporting-azure-resources-for-your-function) to set up your Azure Function resources.

## 🌟 Use Cases
This function demonstrates how to extend Azure OpenAI models for various natural language processing tasks. It can be used to:

- 📝 **Automate Content Creation**: Summarize long texts for automated content creation.
- 🔍 **Data Extraction**: Extract useful information from unstructured data.
- 🖼️ **Visual Content Analysis**: Automatically generate captions for images, useful in content management systems.

## 🔧 Extending the Function
You can modify this function to add more scenarios or support other Large Language Models (LLMs). For example:

- **Add New Skills**: Add more scenarios like **text classification** or **sentiment analysis** by modifying the `ScenarioType` and adding new handling logic in the `prepare_messages` function.
- **Swap Models**: Replace the deployed model with another capable model from Azure AI Studio that fits your use case.

## ❓ FAQ

### How do I add a new skill to the function?
To add a new skill, update the `ScenarioType` enumeration with a new value and extend the `prepare_messages()` function to handle the new scenario appropriately.

### Can I use models other than `gpt-4o`?
Yes, you can replace the deployed model with any suitable model from Azure AI Studio. Make sure to update the endpoint and model name in your `local.settings.json` file.

### How do I use Azure AI Studio models?
For other models that Azure AI Studio offers, simply change the **deployment name** and **endpoint URL** in the configuration to the desired model, ensuring compatibility with the tasks.

## 📚 Documentation
- [Azure OpenAI Service Documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)

## 🛠️ Technologies Used
- **Python**
- **Azure Functions**
- **Azure AI Studio**

---

Feel free to explore, modify, and extend the functionality as needed for your use case. If you have questions or suggestions, don't hesitate to open an issue or contribute to the repository!
