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
        poller = form_recognizer_client.begin_recognize_receipts_from_url(form_url)
        result = poller.result()
        items_list = []
        invoice = []
        for receipt in result:
            for name, field in receipt.fields.items():
                if name == "Items":
                    print("Receipt Items:")
                    for idx, items in enumerate(field.value):
                        print("...Item #{}".format(idx + 1))
                        for item_name, item in items.value.items():
                            items_list.append(
                                {
                                    "name": item_name,
                                    "value": item.value,
                                    "confidence": item.confidence
                                }
                            )
                            print("......{}: {} has confidence {}".format(item_name, item.value, item.confidence))
                else:
                    print("{}: {} has confidence {}".format(name, field.value, field.confidence))
                    invoice.append(
                                {
                                    "name": name,
                                    "value": field.value,
                                    "confidence": field.confidence
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
                "invoice": invoice,
                "items": items_list
            }
            })