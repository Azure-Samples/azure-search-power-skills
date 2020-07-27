import logging
import azure.functions as func
import json
import cv2
import numpy as np
import base64

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
    return json.dumps(results, ensure_ascii=False)

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
        assert ('image' in data), "'image' field is required in 'data' object"
        image = value['data']['image']
        assert ('$type' in image), "'$type' field is required in 'image' object."
        assert (image['$type'] == 'file'), "'$type' field must be 'file' in 'image' object."
        assert ('data' in image), "'data' field is required in 'image' object."
    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })

    try:                
        image_bytes = decode_image_bytes(image['data'])
        image_np = np.frombuffer(image_bytes, dtype=np.uint8)
        image_cv = cv2.imdecode(image_np, cv2.IMREAD_COLOR)
        # Converting the image to grayscale.
        gray = cv2.cvtColor(image_cv, cv2.COLOR_BGR2GRAY)

        # Smoothing without removing edges.
        gray_filtered = cv2.bilateralFilter(gray, 7, 50, 50)

        # Applying the canny filter
        edges_filtered = cv2.Canny(gray_filtered, 60, 120)

        is_success, output = cv2.imencode(".png", edges_filtered)
    except Exception as e:
        result = str(e)
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Could not complete operation for record. Error: " + result }   ]       
            })

    return ({
            "recordId": recordId,
            "data": {
                "image": {
                    "$type": "file",
                    "name": "output.png",
                    "data": encode_image_bytes(output)
                }
            }
        })

def decode_image_bytes(image_data):
    base64_bytes = image_data.encode('utf-8')
    return base64.b64decode(base64_bytes)

def encode_image_bytes(image_data):
    base64_bytes = base64.b64encode(image_data)
    return base64_bytes.decode('utf-8')