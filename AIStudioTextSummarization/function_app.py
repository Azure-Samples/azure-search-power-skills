import azure.functions as func
import datetime
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
    