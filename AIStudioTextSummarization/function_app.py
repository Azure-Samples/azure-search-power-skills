import azure.functions as func
import json
import logging
import requests
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
    call_chat_completion_model() # pass in the actual payload later
    logging.info("Sucessfully returned the response body!")
    return response


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
# TODO: figure out how to add this into a different file later
def call_chat_completion_model():
    # Configuration
    API_KEY = "YOUR_API_KEY"
    # IMAGE_PATH = "YOUR_IMAGE_PATH"
    # encoded_image = base64.b64encode(open(IMAGE_PATH, 'rb').read()).decode('ascii')
    headers = {
        "Content-Type": "application/json",
        "api-key": API_KEY,
    }

    # Payload for the request
    payload = {
    "messages": [
        {
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
        "content": [
            {
            "type": "text",
            "text": "OpenAI just closed a historic funding round, taking in a $6.6 billion investment at a $157 billion valuation, to continue pursuing its mission to build artificial-general intelligence according to a company blog post. The funding round was led by Thrive Capital, which committed $1 billion, according to the Financial Times. It was also reported that Thrive got a special deal (not offered to other investors) that allows it to invest another $1 billion next year at the same valuation if the AI firm hits a revenue goal, Reuters reported. These funds are apparently contingent on OpenAI going through with a rumored restructure as a for-profit company. The company's for-profit wing is currently overseen by a nonprofit research body, and investor profits are capped at 100x. If OpenAI doesn't restructure itself as a for-profit company within two years, Axios reported, investors can ask for their money back. Last week, Reuters reported that the company is considering becoming a public benefit corporation (like Anthropic). In a rare move, OpenAI also asked investors to avoid backing rival start-ups such as Anthropic and Elon Musk's xAI, the Financial Times reported. It's worth noting that OpenAI's latest funding round just barely surpasses xAI, which raised $6 billion in May.These billions will go toward the incredibly expensive task of training AI frontier models. Anthropic CEO Dario Amodei has said AI models that cost $1 billion to train are in development and $100 billion models are not far behind. For OpenAI, which wants to build a series of “reasoning” models, those costs are only expected to balloon — making fresh funding rounds like this one critical."
            }
        ]
        }
    ],
    "temperature": 0.7,
    "top_p": 0.95,
    "max_tokens": 4096
    }

    ENDPOINT = "https://azs-grok-aoai.openai.azure.com/openai/deployments/azs-grok-gpt-4o/chat/completions?api-version=2024-02-15-preview"

    # Send request
    try:
        response = requests.post(ENDPOINT, headers=headers, json=payload)
        response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
    except requests.RequestException as e:
        raise SystemExit(f"Failed to make the request. Error: {e}")

    # Handle the response as needed (e.g., print or process)
    print(response.json())