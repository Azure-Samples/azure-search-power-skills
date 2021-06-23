import base64
import io
import logging
import os
from heapq import nsmallest

import numpy as np
from PIL import Image
from dotenv import load_dotenv
from tensorflow.keras.applications.resnet50 import preprocess_input
from tensorflow.keras.models import Model
from tensorflow.keras.preprocessing import image
from objdict import ObjDict
from powerskill.timer import timefunc
from scipy.spatial import distance

load_dotenv()


def find_most_similar(image_vectors, all_image_features):
    """

    Parameters
    ----------
    image_vectors: Vectors of our input image
    all_image_features: All vectorised images

    Returns: The cosine similarity score per comparison
    -------

    """
    scorescos = {}
    for key, vector in all_image_features.items():
        scorecos = findDifference(image_vectors, vector)
        scorescos[key] = scorecos  # cosine similarity

    return scorescos


def predict(img64: str, model: Model):
    """

    Parameters
    ----------
    img64: The base64 encoded representation of the image
    model: The ResNet model

    Returns: The extracted features
    -------

    """
    # Load the image
    temp_image = Image.open(io.BytesIO(img64))
    newsize = (224, 224)
    im = temp_image.resize(newsize)
    x = image.img_to_array(im)
    x = np.expand_dims(x, axis=0)
    x = preprocess_input(x)
    return model.predict(x)


def findDifference(image_vector1, image_vector2):
    """

    Parameters
    ----------
    image_vector1: Our source image vector
    image_vector2: The target image vector

    Returns: Cosine distance score
    -------

    """
    dist = distance.cdist(
        image_vector1.reshape(1, -1),
        image_vector2.reshape(1, -1),
        metric="cosine")
    return dist[0][0]


def extract_image_features(resnet_model, image64):
    """

    Parameters
    ----------
    resnet_model: The ResNet model for feature extraction
    image64: The base64 encoded representation of the image

    Returns: Extracted features
    -------

    """
    # Here we extract the features of the image
    image_vectors = predict(image64, resnet_model)[0]
    return image_vectors


def set_log_level(debug):
    """
    :param debug: Boolean value
    :return: None
    """
    if bool(debug):
        logging.basicConfig(level=logging.DEBUG)


set_log_level(bool(os.environ['DEBUG']))


def build_output_response(inputs, outputs, topncos, error=None):
    """

    :param inputs: The inputs gathered from the extraction process
    :param outputs: The outputs object - power skill output
    :return: The json response object
    """
    values = ObjDict()
    values.values = []
    entities = []

    entities.append(topncos)
    entity_values = {}
    entity_values['most_similar'] = topncos
    errors = ''
    values.values.append({'recordId': inputs['values'][0]['recordId'], \
                          "errors": errors,
                          "data": entity_values,
                          "warnings": ""})

    return values


@timefunc
def go_extract(inputs, all_image_features, resnet_model, topn):
    """
    :param args:
    :return:
    """

    try:
        outputs = {}
        output_response = {}
        topncos = {}

        record_id = inputs['values'][0]['recordId']
        # Get the base64 encoded image
        encoded_image = inputs['values'][0]['data']['images']['data']
        img = base64.b64decode(str(encoded_image).strip())
        logging.info((f"Base64Encoded string {img[:100]}"))
        image_vectors = extract_image_features(resnet_model, img)
        compared_vectorscos = find_most_similar(image_vectors, all_image_features)

        topncos = nsmallest(topn, compared_vectorscos, key=compared_vectorscos.get)

    except Exception as ProcessingError:
        logging.exception(ProcessingError)
        error = str(ProcessingError)
        output_response = build_output_response(inputs, outputs, topncos)

    logging.info(output_response)

    output_response = build_output_response(inputs, outputs, topncos)
    return output_response
