import logging
import azure.functions as func
import cv2
import numpy as np
from ..utilities import webapiskill
from ..utilities.opencvskill import opencvskill

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')
    return webapiskill.process_request_records(req, transform_data)

## Perform an operation on a record
def transform_data(data):
    # Validate the inputs
    try:
        assert ('image' in data), "'image' field is required in 'data' object"
        image = data['image']
        assert ('$type' in image), "'$type' field is required in 'image' object."
        assert (image['$type'] == 'file'), "'$type' field must be 'file' in 'image' object."
        assert ('data' in image), "'data' field is required in 'image' object."
    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
        })

    image_bytes = webapiskill.decode_image_bytes(image['data'])
    image_np = np.frombuffer(image_bytes, dtype=np.uint8)
    cv_skill = opencvskill(debug_override=True)
    image_cv = cv_skill.imdecode(image_np, cv2.IMREAD_COLOR)
    # Converting the image to grayscale.
    gray = cv_skill.cvtColor(image_cv, cv2.COLOR_BGR2GRAY)
    # Smoothing without removing edges.
    gray_filtered = cv_skill.bilateralFilter(gray, 7, 50, 50)
    # Applying the canny filter
    edges_filtered = cv_skill.Canny(gray_filtered, 60, 120)

    output_debug_steps = cv_skill.get_output_debug_steps()
    return {
        "images": output_debug_steps
    }