import azure.functions as func
import json
import logging
import requests
import os

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
@app.function_name(name="AOAICustomSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("calling the aoai custom skill endpoint")
    request_json = dict(req.get_json())
    # logging.info(f"the request json is: {request_json}")
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
    headers = {
        "Content-Type": "application/json",
        "api-key": api_key,
    }
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
        raw_image_data = request_body.get("data", {}).get("image", "")
        # logging.info(f"the raw image data is: {raw_image_data}")
        # need to contruct this prefix - data:image/jpeg;base64,
        #TODO: we can get the EXACT image type from the raw image data
        image_data = raw_image_data.get("data")
        image_type = raw_image_data.get("contentType")
        image_base64encoded = f'data:{image_type};base64,{image_data}'
        # image_base64encoded = request_body.get("data", {}).get("image", "")
        logging.info(f"the image base64 encoded is: {image_base64encoded}")
        messages = [ {
            "role": "system",
            "content": 
            [
                {
                    "type": "text",
                    "text": custom_prompts.get("image-captioning-simple-description-prompt")
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
                    "text": "I want you to describe this image in 1-2 simple sentences."
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

    ENDPOINT = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
    
    try:
        response = requests.post(ENDPOINT, headers=headers, json=request_payload)
        response.raise_for_status()
    except requests.RequestException as e:
        raise SystemExit(f"Failed to make the request. Error: {e}")

    response_json = response.json()
    top_response_text = response_json['choices'][0]['message']['content']
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
    logging.info(f"the response body is: {response_body}")
    return response_body