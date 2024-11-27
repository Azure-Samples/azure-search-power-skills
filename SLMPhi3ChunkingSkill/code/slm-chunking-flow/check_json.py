import json
from azure.ai.inference import ChatCompletionsClient
from azure.ai.inference.models import SystemMessage, UserMessage
from azure.core.credentials import AzureKeyCredential
from promptflow.core import tool

endpoint = "https://models.inference.ai.azure.com"
model_name = "Phi-3.5-mini-instruct"
token =  'Your GitHub Model Token'


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def check_json(content: str) -> str:
    client = ChatCompletionsClient(
        endpoint=endpoint,
        credential=AzureKeyCredential(token),
    )

    response = client.complete(
        messages=[
            SystemMessage(content="""
You are a json checker, please make sure the input is in json array format, please adjust the incorrect json format and output as json array format. If correct, please output the input directly.
                        """),
            UserMessage(content=content),
        ],
        temperature=0.1,
        top_p=1.0,
        max_tokens=100000,
        model=model_name
    )
    return response.choices[0].message.content.replace('\n','')
