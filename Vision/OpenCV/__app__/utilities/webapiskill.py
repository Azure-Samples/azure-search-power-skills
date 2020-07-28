import azure.functions as func
import json
import base64

def process_request_records(req, transform_data):
    try:
        json_data = req.get_json()
    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
    
    values = json_data['values']
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    
    for value in values:
        output_record = process_record(value, transform_data)
        if output_record != None:
            results["values"].append(output_record)
    result = json.dumps(results, ensure_ascii=False)
    return func.HttpResponse(result, mimetype="application/json")

def process_record(value, transform_data):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None

    # Validate the inputs
    try:
        assert ('recordId' in value), "'recordId' field is required."
        assert ('data' in value), "'data' field is required."
        recordId = value['recordId']
        data = value['data']
        return {
            "recordId": recordId,
            "data": transform_data(data)
        }
    except Exception as e:
        result = str(e)
        return  {
            "recordId": recordId,
            "errors": [ { "message": "Could not complete operation for record. Error: " + result } ]       
        }

def decode_image_bytes(image_data):
    base64_bytes = image_data.encode('utf-8')
    return base64.b64decode(base64_bytes)

def encode_image_bytes(image_data):
    base64_bytes = base64.b64encode(image_data)
    return base64_bytes.decode('utf-8')