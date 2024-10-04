import azure.functions as func
import json
import logging
import requests
import os
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
    for request_body in input_values:
      api_response = call_chat_completion_model(request_body, api_key) # pass in the actual payload later
      response_values.append(api_response)
    response_body = { "values": response_values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'
    return response

# TODO: figure out how to add this into a different file later
def call_chat_completion_model(request_body: dict, api_key: str):
    headers = {
        "Content-Type": "application/json",
        "api-key": api_key,
    }
    # get the exact user prompt message here
    user_prompt_content = {
            "type": "text",
            "text": request_body.get("data", {}).get("text", "")
    }

    messages = [
        { # Note: this is a sample prompt which can be tweaked according to your exact needs
        "role": "system",
        "content": [
            {
            "type": "text",
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

    logging.info(f"the new request payload is: {request_payload}")
    ENDPOINT = "https://azs-grok-aoai.openai.azure.com/openai/deployments/azs-grok-gpt-4o/chat/completions?api-version=2024-02-15-preview"
    # Send request
    try:
        response = requests.post(ENDPOINT, headers=headers, json=request_payload)
        response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
    except requests.RequestException as e:
        raise SystemExit(f"Failed to make the request. Error: {e}")

    # Handle the response as needed (e.g., print or process)
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