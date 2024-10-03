import azure.functions as func
import json
import logging
import os
from azure.ai.inference import ChatCompletionsClient
from azure.core.credentials import AzureKeyCredential
# from AIStudioTextSummarization.chat_completion import call_chat_completion_model
# import jsonschema # only works on newer Python version for some reason...

app = func.FunctionApp()

# the healthcheck endpoint. Important to make sure that deployments are healthy
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
def HealthCheck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Calling the healthcheck endpoint')
    response_body = { "status": "Healthy" }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'    
    return response

'''
the sample payload for the summarization skill will look like this:
{
    "values": [
      {
        "recordId": "0",
        "data":
           {
             "text": "Este es un contrato en Inglés",
             "language": "es",
             "phraseList": ["Este", "Inglés"]
           }
      },
      {
        "recordId": "1",
        "data":
           {
             "text": "Hello world",
             "language": "en",
             "phraseList": ["Hi"]
           }
      },
      {
        "recordId": "2",
        "data":
           {
             "text": "Hello world, Hi world",
             "language": "en",
             "phraseList": ["world"]
           }
      },
      {
        "recordId": "3",
        "data":
           {
             "text": "Test",
             "language": "es",
             "phraseList": []
           }
      }
    ]
}
'''

@app.function_name(name="TextSummarizer")
@app.route(route="summarize")
def text_chunking(req: func.HttpRequest) -> func.HttpResponse:
    request_json = dict(req.get_json())
    input_values = []
    try:
      headers_as_dict = dict(req.headers)
      scenario = headers_as_dict.get("scenario")
      if scenario != "summarization":
          raise ValueError(f"incorrect scenario in header. Expected 'summarization', but got {scenario}")
      input_values = request_json.get("values")
      if not input_values:
          raise ValueError(f"expected values in the request body, but got {input_values}")
    except ValueError as value_error:
        return func.HttpResponse("Invalid request: {0}".format(value_error), status_code=400)
    # logging.info(f"got input values as: {input_values}")
    response_values = []
    response_body = { "values": response_values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'
    logging.info("Sucessfully returned the response body!")
    call_chat_completion_model()
    return response


def call_chat_completion_model():
    # TODO: customize this for summarization
    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL", '')
    if not api_key:
        raise Exception("A key should be provided to invoke the endpoint")

    client = ChatCompletionsClient(
        endpoint='https://azs-grok-phi-3-5-vision.eastus.models.ai.azure.com',
        credential=AzureKeyCredential(api_key)
    )

    model_info = client.get_model_info()
    print("Model name:", model_info.model_name)
    print("Model type:", model_info.model_type)
    print("Model provider name:", model_info.model_provider_name)

    payload = {
    "messages": [
        {
        "role": "user",
        "content": "I am going to Paris, what should I see?"
        },
        {
        "role": "assistant",
        "content": "Paris, the capital of France, is known for its stunning architecture, art museums, historical landmarks, and romantic atmosphere. Here are some of the top attractions to see in Paris:\n\n1. The Eiffel Tower: The iconic Eiffel Tower is one of the most recognizable landmarks in the world and offers breathtaking views of the city.\n2. The Louvre Museum: The Louvre is one of the world's largest and most famous museums, housing an impressive collection of art and artifacts, including the Mona Lisa.\n3. Notre-Dame Cathedral: This beautiful cathedral is one of the most famous landmarks in Paris and is known for its Gothic architecture and stunning stained glass windows.\n\nThese are just a few of the many attractions that Paris has to offer. With so much to see and do, it's no wonder that Paris is one of the most popular tourist destinations in the world."
        },
        {
        "role": "user",
        "content": "What is so great about #1?"
        }
    ],
    "max_tokens": 2048,
    "temperature": 0,
    "top_p": 1
    }
    response = client.complete(payload)

    print("Response:", response.choices[0].message.content)
    print("Model:", response.model)
    print("Usage:")
    print("	Prompt tokens:", response.usage.prompt_tokens)
    print("	Total tokens:", response.usage.total_tokens)
    print("	Completion tokens:", response.usage.completion_tokens)


# this function describes the expected schema of the json payload
# for more information, go here: https://learn.microsoft.com/en-us/azure/search/cognitive-search-custom-skill-web-api
'''
def get_summarization_request_schema() -> dict:
    return {
        "$schema": "http://json-schema.org/draft-04/schema#",
        "type": "object",
        "properties": {
            "values": {
                "type": "array",
                "minItems": 1,
                "items": {
                    "type": "object",
                    "properties": {
                        "recordId": {"type": "string"},
                        "data": {
                            "type": "object",
                            "properties": {
                                "text": {"type": "string", "minLength": 1}
                            },
                            "required": ["text"],
                        },
                    },
                    "required": ["recordId", "data"],
                },
            }
        },
        "required": ["values"],
    }
'''