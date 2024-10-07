import os
import azure.functions as func
import logging
import json
import jsonschema
from azure.ai.inference import ChatCompletionsClient
from azure.core.credentials import AzureKeyCredential

app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)

# A healthcheck endpoint. Important to make sure that deployments are healthy.
# It can be accessed via <base_url>/api/health
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
def HealthCheck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Calling the healthcheck endpoint")
    response_body = {"status": "Healthy"}
    response = func.HttpResponse(
        json.dumps(response_body, default=lambda obj: obj.__dict__)
    )
    response.headers["Content-Type"] = "application/json"
    return response


@app.function_name(name="EntityRecognition")
@app.route(route="entity_recognition")
def entity_recognition(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("calling the summarize endpoint")
    request_json = dict(req.get_json())
    input_values = []
    api_key = None
    try:
        jsonschema.validate(request_json, schema=get_request_schema())
        headers_as_dict = dict(req.headers)
        scenario = headers_as_dict.get("scenario")
        if scenario != "entity-recognition":
            raise ValueError(
                f"incorrect scenario in header. Expected 'entity-recognition', but got {scenario}"
            )
        input_values = request_json.get("values")
        if not input_values:
            raise ValueError(
                f"expected values in the request body, but got {input_values}"
            )
        api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
        if not api_key:
            raise ValueError(
                f"expected an api key from env variable - AZURE_INFERENCE_CREDENTIAL, but got: {api_key}"
            )
    except jsonschema.exceptions.ValidationError as e:
        return func.HttpResponse("Invalid request: {0}".format(e), status_code=400)
    except ValueError as value_error:
        return func.HttpResponse(
            "Invalid request: {0}".format(value_error), status_code=400
        )
    response_values = []
    # TODO: this should be parallelized in the future for performance improvements since we don't need the requests to occur serially
    for request_body in input_values:
        api_response = call_chat_completion_model(
            request_body, api_key
        )  # pass in the actual payload later
        response_values.append(api_response)
    response_body = {"values": response_values}
    response = func.HttpResponse(
        json.dumps(response_body, default=lambda obj: obj.__dict__)
    )
    response.headers["Content-Type"] = "application/json"
    return response


# TODO: figure out how to add this into a different file later. It's currently causing interpreter errors when running locally.
def call_chat_completion_model(request_body: dict, api_key: str):
    client = ChatCompletionsClient(
        endpoint='https://azs-grok-phi-3-5-vision.eastus.models.ai.azure.com',
        credential=AzureKeyCredential(api_key)
    )

    model_info = client.get_model_info()
    print("Model name:", model_info.model_name)
    print("Model type:", model_info.model_type)
    print("Model provider name:", model_info.model_provider_name)

    headers = {
        "Content-Type": "application/json",
        "api-key": api_key,
    }
    user_prompt_content = {
        "type": "text",
        "text": request_body.get("data", {}).get("text", ""),
    }
    messages = [
        {
            "role": "system",
            "content": [
                {
                    "type": "text",
                    # Note: this is a sample prompt which can be tweaked according to your exact needs
                    "text": "You are a useful AI assistant. I need you to help me recognize entities in JSON format. From the text given to you, identity all people names, addresses, email addresses, engineering job titles and present them as individual lists in a JSON object.",
                }
            ],
        },
        {"role": "user", "content": [user_prompt_content]},
    ]

    request_payload = {
        "messages": messages,
        "temperature": 0.7,
        "top_p": 0.95,
        "max_tokens": 4096,
    }

    # this stuff should be different
    ENDPOINT = "https://azs-grok-aoai.openai.azure.com/openai/deployments/azs-grok-gpt-4o/chat/completions?api-version=2024-02-15-preview"

    try:
        response = requests.post(ENDPOINT, headers=headers, json=request_payload)
        response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
    except requests.RequestException as e:
        raise SystemExit(f"Failed to make the request. Error: {e}")

    response_json = response.json()
    top_response_text = response_json["choices"][0]["message"]["content"]
    response_body = {
        "warnings": None,
        "errors": [],
        "recordId": request_body.get("recordId"),
        "data": None,
    }
    response_body["data"] = {"generative-summary": top_response_text}
    return response_body


def get_request_schema():
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
                            "properties": {"text": {"type": "string", "minLength": 1}},
                            "required": ["text"],
                        },
                    },
                    "required": ["recordId", "data"],
                },
            }
        },
        "required": ["values"],
    }
