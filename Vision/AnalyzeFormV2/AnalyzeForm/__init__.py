import logging
import json
import os
import logging
import pathlib
from azure.core.exceptions import ResourceNotFoundError
from azure.ai.formrecognizer import FormRecognizerClient
from azure.ai.formrecognizer import FormTrainingClient
from azure.core.credentials import AzureKeyCredential
import azure.functions as func


endpoint = os.environ["fr_endpoint"]
key = os.environ["fr_key"]
model_id = os.environ["model_id"]

form_recognizer_client = FormRecognizerClient(endpoint, AzureKeyCredential(key))

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    try:
        body = json.dumps(req.get_json())
        if endpoint is None or key is None or model_id is None:
            return func.HttpResponse(
             "Skill configuration error. Endpoint, key and model_id required.",
             status_code=400
        )

    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
    except KeyError:
        return func.HttpResponse(
             "Skill configuration error. Endpoint, key and model_id required.",
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
    mappings = None
    with open(pathlib.Path(__file__).parent / 'fieldmappings.json') as file:
        mappings = json.loads(file.read())
    for value in values:
        output_record = transform_value(value, mappings)
        if output_record != None:
            results["values"].append(output_record)
    return json.dumps(results, ensure_ascii=False)

## Perform an operation on a record
def transform_value(value, mappings):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None

    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']        
        
        formUrl = data['formUrl']
        formSasToken = data ['formSasToken']

        formUrl = formUrl + formSasToken

        poller = form_recognizer_client.begin_recognize_custom_forms_from_url(
        model_id=model_id, form_url=formUrl)
        result = poller.result()
        recognized = {}
        for recognized_form in result:
            print("Form type: {}".format(recognized_form.form_type))
            for name, field in recognized_form.fields.items():
                label = field.label_data.text if field.label_data else name
                for (k, v) in mappings.items(): 
                    if(label == k):
                        recognized[label] =  field.value 

    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })
    except Exception as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + str(error) }   ]       
            })


    

    return ({
            "recordId": recordId,   
            "data": {
                "recognized": recognized
            }
            })