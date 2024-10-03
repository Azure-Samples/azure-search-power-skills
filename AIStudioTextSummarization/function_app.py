import azure.functions as func
import json
import logging
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
    logging.info('Received a request to the summarize endpoint.')
    request_json = req.get_json()
    try:
      headers_as_dict = dict(req.headers)
      scenario = headers_as_dict.get("scenario")
      if scenario != "summarization":
          # throw an error here
          raise ValueError(f"incorrect scenario in header. Expected 'summarization', but got {scenario}")
    except ValueError as value_error:
        return func.HttpResponse("Invalid request: {0}".format(value_error), status_code=400)
    values = []
    response_body = { "values": values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'
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