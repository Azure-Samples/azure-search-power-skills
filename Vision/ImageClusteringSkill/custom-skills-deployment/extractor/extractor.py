import logging
import os
import sys

sys.path.append("../")

import joblib
from PIL import Image
from dotenv import load_dotenv
from extractor.timer import timefunc
from numpy import asarray
from objdict import ObjDict
import requests
import io
import base64

from sklearn.utils.estimator_checks import check_estimator

__vgg16_extractor__ = 'vgg16_extractor.py'
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__vgg16_extractor__), '.. ..')))

from ml.extractors.vgg16_extractor import VGG16Extractor
from ml.models.DBSCAN import DBSCANv2
from ml.similarity.detector import ImageSimilarityDetector

load_dotenv()
sample_model = False

def set_log_level(debug):
    """
    :param debug: Boolean value
    :return: None
    """
    if bool(debug):
        logging.basicConfig(level=logging.DEBUG)

set_log_level(os.getenv('DEBUG', 'False'))

# Let's load all models upfront
# Load DBSCAN model from registry
try:
    model = joblib.load(os.path.join("models/", os.environ['DBSCAN_MODEL']))
    logging.info(f"Loaded model {os.environ['DBSCAN_MODEL']}")
except Exception as NoModel:
    sample_model = True
    # Create DBSCANv2 instance and verify whether the custom class is correct
    model = DBSCANv2(eps=0.3, min_samples=1, metric="cosine")
    logging.info(f"Using sample model")

# Create VGG16 extractor
extractor = VGG16Extractor()

check_estimator(DBSCANv2)

# Create similarity detector
detector = ImageSimilarityDetector(extractor, model)


def build_output_response(recordId, label, error, cluster_labels):
    """

    :param inputs: The inputs gathered from the extraction process
    :param outputs: The outputs object - power skill output
    :param cluster_labels: The provided labels for the clusters
    :return: The json response object
    """
    values = ObjDict()
    values.values = []
    entity_values = {}

    if len(cluster_labels) > 0:
        entity_values['label'] = label
    else:
        if len(label) > 0:
            entity_values['label'] = int(label[0])
        else:
            entity_values['label'] = ''

    if len(error) > 0:
        errors = [error]
    else:
        errors = ""

    values.values.append({"recordId": recordId, \
                          "errors": errors,
                          "data": entity_values,
                          "warnings": ""})

    return values


@timefunc
def go_extract(inputs):
    """
    Our main function to predict the cluster for an image
    Args:
        inputs: json

    Returns:

    """
    try:
        label = ''
        error = ''
        cluster_labels = []

        record_id = inputs['values'][0]['recordId']
        # Get the base64 encoded image
        encoded_image = inputs['values'][0]['data']['images']['data']

        img = base64.b64decode(str(encoded_image).strip())

        logging.info(f"Cluster labels file {os.environ.get('CLUSTER_LABELS')}")
        cluster_labels = joblib.load(os.path.join("models/", os.environ.get("CLUSTER_LABELS")))
        logging.info(f"Loaded cluster labels {cluster_labels}")
        # We will run on a small sample dataset
        if sample_model:
            # Download sample data
            from sklearn.datasets import load_sample_images, load_sample_image

            dataset = load_sample_images()
            images = dataset['images']
            images.extend(images)

            # Train detector
            labels = detector.train(images)

        # Load the image
        image = Image.open(io.BytesIO(img))
        # Convert image to numpy array
        img = asarray(image)
        # Predict
        label = detector.assign_group([img])
        logging.info(f"Predicted cluster {label.item()} recordId {record_id}")

        if len(cluster_labels) > 0 and label.item() > -1:
            label = cluster_labels[label.item()]
        else:
            label = ''

        output_response = build_output_response(record_id, label, error, cluster_labels)

    except Exception as ProcessingError:
        logging.exception(ProcessingError)
        error = str(ProcessingError)
        output_response = build_output_response(record_id, label, error, cluster_labels)

    logging.info(output_response)

    return output_response
