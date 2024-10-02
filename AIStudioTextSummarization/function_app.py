import azure.functions as func
import json
import logging

app = func.FunctionApp()

@app.route(route="HttpExample", auth_level=func.AuthLevel.ANONYMOUS)
def HttpExample(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')
    name = req.params.get('name')
    if not name:
        name = "SOME_DEFAULT_NAME"
    return func.HttpResponse(f"Hello, {name}. This HTTP triggered function executed successfully.")

# the healthcheck endpoint. Important to make sure that deployments are healthy
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
def HealthCheck(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Calling the healthcheck endpoint')
    response_body = { "status": "Healthy" }
    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'    
    return response