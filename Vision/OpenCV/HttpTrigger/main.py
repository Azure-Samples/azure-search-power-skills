import logging
import azure.functions as func
import cv2
import numpy as np
import webapiskill

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
    image_cv = cv2.imdecode(image_np, cv2.IMREAD_COLOR)
    # Converting the image to grayscale.
    gray = cv2.cvtColor(image_cv, cv2.COLOR_BGR2GRAY)

    # Smoothing without removing edges.
    gray_filtered = cv2.bilateralFilter(gray, 7, 50, 50)

    # Applying the canny filter
    edges_filtered = cv2.Canny(gray_filtered, 60, 120)

    def hconcat_resize_min(im_list, interpolation=cv2.INTER_CUBIC):
        h_min = min(im.shape[0] for im in im_list)
        im_list_resize = [cv2.resize(im, (int(im.shape[1] * h_min / im.shape[0]), h_min), interpolation=interpolation)
                        for im in im_list]
        return cv2.hconcat(im_list_resize)

    def vconcat_resize_min(im_list, interpolation=cv2.INTER_CUBIC):
        w_min = min(im.shape[1] for im in im_list)
        im_list_resize = [cv2.resize(im, (w_min, int(im.shape[0] * w_min / im.shape[1])), interpolation=interpolation)
                        for im in im_list]
        return cv2.vconcat(im_list_resize)
    
    combined_images_top = hconcat_resize_min([image_cv, cv2.cvtColor(gray, cv2.COLOR_GRAY2BGR)])
    combined_images_bottom = hconcat_resize_min([cv2.cvtColor(gray_filtered, cv2.COLOR_GRAY2BGR), cv2.cvtColor(edges_filtered, cv2.COLOR_GRAY2BGR)])
    combined_images = vconcat_resize_min([combined_images_top, combined_images_bottom])

    is_success, output = cv2.imencode(".png", combined_images)
    return {
        "image": {
            "$type": "file",
            "name": "output.png",
            "data": webapiskill.encode_image_bytes(output)
        }
    }