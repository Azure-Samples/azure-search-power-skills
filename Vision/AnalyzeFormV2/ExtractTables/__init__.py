import logging
import json
import os
import logging
import datetime
from json import JSONEncoder

import azure.functions as func
from azure.ai.formrecognizer import FormRecognizerClient
from azure.ai.formrecognizer import FormTrainingClient
from azure.core.credentials import AzureKeyCredential


endpoint = os.environ["fr_endpoint"]
key = os.environ["fr_key"]

form_recognizer_client = FormRecognizerClient(endpoint, AzureKeyCredential(key))

class DateTimeEncoder(JSONEncoder):
        #Override the default method
        def default(self, obj):
            if isinstance(obj, (datetime.date, datetime.datetime)):
                return obj.isoformat()


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        body = json.dumps(req.get_json())
    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
    
    if body:
        logging.info(body)
        result = compose_response(body)
        return func.HttpResponse(result, mimetype="application/json")
    else:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )


def compose_response(json_data):
    values = json.loads(json_data)['values']
    
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    
    for value in values:
        output_record = transform_value(value)
        if output_record != None:
            results["values"].append(output_record)
    return json.dumps(results, ensure_ascii=False, cls=DateTimeEncoder)

## Perform an operation on a record
def transform_value(value):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None

    # Validate the inputs
    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']   
        print(data)
        form_url = data["formUrl"]  + data["formSasToken"]   
        print(form_url)
        poller = form_recognizer_client.begin_recognize_content_from_url(form_url)
        page = poller.result()
        cells = []
        table = page[0].tables[0] # page 1, table 1
        print("Table found on page {}:".format(table.page_number))
        for cell in table.cells:
            cells.append(
                {
                    "text": cell.text,
                    "rowIndex": cell.row_index,
                    "colIndex": cell.column_index,
                    "confidence": cell.confidence
                }
            )
    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })

    

    return ({
            "recordId": recordId,   
            "data": {
                "table": cells
            }
            })