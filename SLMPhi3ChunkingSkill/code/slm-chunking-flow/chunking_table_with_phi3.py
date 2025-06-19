import os
import json
from azure.ai.inference import ChatCompletionsClient
from azure.ai.inference.models import SystemMessage, UserMessage
from azure.core.credentials import AzureKeyCredential
from promptflow.core import tool

endpoint = "https://models.inference.ai.azure.com"
model_name = "Phi-3.5-mini-instruct"
token =  'Your GitHub Model Token'


@tool
def chunking_table(tables: list) -> list:

    client = ChatCompletionsClient(
        endpoint=endpoint,
        credential=AzureKeyCredential(token),
    )

    tableList = []

    for item in tables:
        table_dict = json.loads(item)
        response = client.complete(
            messages=[
                SystemMessage(content="""You are my markdown table assistant, who can understand all the contents of the table and give analysis."""),
                UserMessage(content=table_dict["md_content"]),
            ],
            temperature=1.0,
            top_p=1.0,
            max_tokens=1000,
            model=model_name
        )
        content_item = { "chunking" :response.choices[0].message.content }
        tableList.append(content_item)

    return tableList
