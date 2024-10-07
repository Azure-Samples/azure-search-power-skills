import azure.functions as func
import logging
import json
import jsonschema


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
    logging.info('Entity Recognition endpoint received a request.')
    request = req.get_json()
    try:
        jsonschema.validate(request, schema=get_request_schema())
    except jsonschema.exceptions.ValidationError as e:
        return func.HttpResponse("Invalid request: {0}".format(e), status_code=400)
    values = []
    response_body = { "values": values }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json' 
    return response

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