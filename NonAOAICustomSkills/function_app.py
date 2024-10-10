import azure.functions as func
import json
import logging
import requests
import os
from azure.ai.inference import ChatCompletionsClient
from azure.core.credentials import AzureKeyCredential

app = func.FunctionApp()

# A healthcheck endpoint. Important to make sure that deployments are healthy.
# It can be accessed via <base_url>/api/health
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
def HealthCheck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Calling the healthcheck endpoint')
    response_body = { "status": "Healthy" }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'   
    return response

# the custom skill endpoint. It can be accessed via <base_url>/api/custom_skill
# TODO: tailor this to work for the phi language model
@app.function_name(name="NonAOAICustomSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("calling the NON-AOAI custom skill endpoint")
    request_json = dict(req.get_json())
    input_values = []
    api_key = None
    try:
      headers_as_dict = dict(req.headers)
      scenario = headers_as_dict.get("scenario")
      input_values = request_json.get("values")
      if not input_values:
          raise ValueError(f"expected values in the request body, but got {input_values}")
      api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
      if not api_key:
          raise ValueError(f"expected an api key from env variable - AZURE_INFERENCE_CREDENTIAL, but got: {api_key}")
    except ValueError as value_error:
        return func.HttpResponse("Invalid request: {0}".format(value_error), status_code=400)
    response_values = []
    # TODO: this can be parallelized in the future for performance improvements since we don't need requests to occur serially
    for request_body in input_values:
      api_response = call_chat_completion_model(request_body=request_body, scenario=scenario)
      response_values.append(api_response)
    response_body = { "values": response_values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'
    return response

def call_chat_completion_model(request_body: dict, scenario: str):
    SUMMARIZATION_HEADER = "summarization"
    ENTITY_RECOGNITION_HEADER = "entity-recognition"
    IMAGE_CAPTIONING_HEADER = "image-captioning"

    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
    ENDPOINT = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
    client = ChatCompletionsClient( endpoint=ENDPOINT, credential=AzureKeyCredential(api_key))
    model_info = client.get_model_info()
    print("Model name:", model_info.model_name)
    print("Model type:", model_info.model_type)
    print("Model provider name:", model_info.model_provider_name)
    # default our chat completion context to be for summarization
    chat_completion_system_context = {}
    messages = []
    custom_prompts = {}
    # read from a json file called custom_prmopts.json to read the prompts for the different scenarios
    with open('custom_prompts.json', 'r') as file:
        custom_prompts = json.load(file)

    if scenario == SUMMARIZATION_HEADER:
        logging.info("calling into the summarization capability")
        chat_completion_system_context = {
        "role": "system",
        "content": [ # this context has to be dynamic according to the request header
            {
                "type": "text",
                # Note: this is a sample summarization prompt which can be tweaked according to your exact needs
                "text": custom_prompts.get("summarize-default-system-prompt")
            }
            ]
        }
        user_prompt_content = {
            "type": "text",
            "text": request_body.get("data", {}).get("text", "")
        }
        messages = [
        chat_completion_system_context,
        {
        "role": "user",
        "content": [user_prompt_content]
        }
    ]
    elif scenario == ENTITY_RECOGNITION_HEADER:
        logging.info("calling into the entity recognition capability")
        chat_completion_system_context = {
        "role": "system",
        "content": [
            {
                    "type": "text",
                    # Note: this is a sample prompt which can be tweaked according to your exact needs
                    "text": custom_prompts.get("entity-recognition-default-system-prompt")
                }
            ]
        }
        user_prompt_content = {
            "type": "text",
            "text": request_body.get("data", {}).get("text", "")
        }
        messages = [
        chat_completion_system_context,
        {
        "role": "user",
        "content": [user_prompt_content]
        }
    ]
    elif scenario == IMAGE_CAPTIONING_HEADER:
        logging.info("calling into the image captioning capability")
        image_base64encoded = request_body.get("data", {}).get("image", "")
        messages = [ {
            "role": "system",
            "content": 
            [
                {
                    "type": "text",
                    "text": custom_prompts.get("image-captioning-machine-info-default-prompt")
                }
            ]
            },
            {
                "role": "user",
                "content": [
                {
                    "type": "image_url",
                    "image_url": {"url": image_base64encoded}
                },
                {
                    "type": "text",
                    "text": "Tell me what this is and what's required to make this."
                },
                ]
            }
            ]

    request_payload = {
    "messages": messages,
    "temperature": 0.7,
    "top_p": 0.95,
    "max_tokens": 4096
    }
    
    response = client.complete(request_payload)
    top_response_text = response.choices[0].message.content
    response_body = {
        'warnings': None,
        'errors': [],
        'recordId': request_body.get('recordId'),
        'data': None
    }
    if scenario == SUMMARIZATION_HEADER:
        response_body["data"] = {"generative-summary": top_response_text}
    elif scenario == ENTITY_RECOGNITION_HEADER:
        response_body["data"] = {"entities": top_response_text}
    elif scenario == IMAGE_CAPTIONING_HEADER:
        response_body["data"] = {"generative-caption": top_response_text}
    return response_body