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
def chunking_text(content: str) -> str:
    client = ChatCompletionsClient(
        endpoint=endpoint,
        credential=AzureKeyCredential(token),
    )

    response = client.complete(
        messages=[
            SystemMessage(content="""You are an expert in content chunking. Please help me chunk user's input text according to the following requirements
1. Truncate the text content into chunks of no more than 300 tokens. 
2. Each chunk part should maintain contextual coherence. The truncated content should be retained in its entirety without any additions or modifications.
2. Each chunked part is output JSON format  { \"chunking\": \"...\" }
3. The final output is a JSON array [{ \"chunking\" : \"...\" },{ \"chunking\" :\"...\"},{ \"chunking\" : \"...\"} ....]
                        """),
            UserMessage(content=content),
        ],
        temperature=0.2,
        top_p=1.0,
        max_tokens=120000,
        model=model_name
    )
    return response.choices[0].message.content.replace('\n','')
