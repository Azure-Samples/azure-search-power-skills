import os

import joblib
from tensorflow.keras.applications.resnet50 import ResNet50


class Models:

    def __init__(self, all_image_features, resnet_model):
        self.all_image_features = all_image_features
        self.resnet_model = resnet_model

    def load_image_features(self, image_features_file):
        all_image_features = joblib.load(os.path.join("models", image_features_file))
        self.all_image_features = all_image_features

    def load_resnet_model(self):
        resnet_model = ResNet50(weights='imagenet')
        self.resnet_model = resnet_model
