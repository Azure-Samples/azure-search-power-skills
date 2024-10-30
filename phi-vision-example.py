# ------------------------------------
# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# ------------------------------------
"""
DESCRIPTION:
    This sample demonstrates how to get a chat completions response from
    the service using a synchronous client.

    This sample assumes the AI model is hosted on a Serverless API or
    Managed Compute endpoint. For GitHub Models or Azure OpenAI endpoints,
    the client constructor needs to be modified. See package documentation:
    https://github.com/Azure/azure-sdk-for-python/blob/main/sdk/ai/azure-ai-inference/README.md#key-concepts

USAGE:
    python sample_chat_completions.py

    Set these two environment variables before running the sample:
    1) AZURE_AI_CHAT_ENDPOINT - Your endpoint URL, in the form 
        https://<your-deployment-name>.<your-azure-region>.models.ai.azure.com
        where `your-deployment-name` is your unique AI Model deployment name, and
        `your-azure-region` is the Azure region where your model is deployed.
    2) AZURE_AI_CHAT_KEY - Your model key (a 32-character string). Keep it secret.
"""
import os
import requests
from azure.ai.inference import ChatCompletionsClient
from azure.ai.inference.models import SystemMessage, UserMessage
from azure.core.credentials import AzureKeyCredential

def sample_chat_completions():
    try:
        endpoint = os.environ["AZURE_CHAT_COMPLETION_ENDPOINT"]
        key = os.environ["AZURE_INFERENCE_CREDENTIAL"]
    except KeyError:
        print("Missing environment variable 'AZURE_AI_CHAT_ENDPOINT' or 'AZURE_AI_CHAT_KEY'")
        print("Set them before running this sample.")
        exit()

    client = ChatCompletionsClient(endpoint=endpoint, credential=AzureKeyCredential(key))
    hardcoded_payload = {"messages":[{"role":"system","content":"You are an AI assistant that helps people find information."},{"role":"user","content":"Mile to feet"}],"max_tokens":2048,"temperature":0.8,"top_p":0.1,"presence_penalty":0,"frequency_penalty":0,"stream": False}
    headers = {
        "Content-Type": "application/json",
        "Authorization": key,
    }
    # THIS ONE WORKS!!
    # response_via_http = requests.post(endpoint, headers=headers, json=hardcoded_payload)
    
    try:
      # THIS ONE FAILED initially.
      response = client.complete(
          messages=[
              SystemMessage(content="You are a helpful assistant."),
              UserMessage(content="How many feet are in a mile?"),
          ]
      )

    except Exception as e:
      print(e)

    print(response.choices[0].message.content)
    # [END chat_completions]


if __name__ == "__main__":
    sample_chat_completions()