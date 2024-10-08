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

# the text summarization endpoint. It can be accessed via <basu_url>/api/summarize
@app.function_name(name="TextSummarizer")
@app.route(route="summarize")
def text_summarization(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("calling the summarize endpoint")
    request_json = dict(req.get_json())
    input_values = []
    api_key = None
    try:
      headers_as_dict = dict(req.headers)
      scenario = headers_as_dict.get("scenario")
      if scenario != "summarization":
          raise ValueError(f"incorrect scenario in header. Expected 'summarization', but got {scenario}")
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
      api_response = call_chat_completion_model(request_body)
      response_values.append(api_response)
    response_body = { "values": response_values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'
    return response

# TODO: figure out how to add this into a different file later. It's currently causing interpreter errors when running locally.
def call_chat_completion_model(request_body: dict):
    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
    logging.info(f'the api key is: {api_key}')
    headers = {
        "Content-Type": "application/json",
        "api-key": api_key,
    }
    user_prompt_content = {
            "type": "text",
            "text": request_body.get("data", {}).get("text", "")
    }
    messages = [
        { 
        "role": "system",
        "content": [
            {
            "type": "text",
            # Note: this is a sample prompt which can be tweaked according to your exact needs
            "text": "You are a useful AI assistant who is an expert at succinctly summarizing long form text into a simple summary. Summarize the text given to you in about 200 words or less."
            }
        ]
        },
        {
        "role": "user",
        "content": [user_prompt_content]
        }
    ]

    request_payload = {
    "messages": messages,
    "temperature": 0.7,
    "top_p": 0.95,
    "max_tokens": 4096
    }

    # this should be an environment variable
    ENDPOINT = "https://azs-grok-aoai.openai.azure.com/openai/deployments/azs-grok-gpt-4o/chat/completions?api-version=2024-02-15-preview"
    
    try:
        response = requests.post(ENDPOINT, headers=headers, json=request_payload)
        response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
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
    response_body["data"] = {"generative-summary": top_response_text}
    return response_body