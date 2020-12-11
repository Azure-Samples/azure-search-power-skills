"""
This module contains an implementation of `VGG16Extractor` class that extracts features from images
using pretrained VGG16 neural networks
"""
from typing import Tuple, List

import numpy as np
import cv2

from tensorflow.keras.applications.vgg16 import preprocess_input, VGG16
from tensorflow.keras.preprocessing.image import img_to_array

from .extractor import Extractor


class VGG16Extractor(Extractor):

    def __init__(self, weights: str = 'imagenet', input_shape: Tuple[int, int, int] = (240, 240, 3)):
        """Instantiates a `VGG16Extractor` class that implements image extraction using
        a VGG16 model pretrained on ImageNet

        Parameters
        ----------
        weights : str, optional
            Path to the pretrained VGG16 weights, by default 'imagenet'
        input_shape : Tuple[int, int], optional
            VGG16 input shape (height, width, channels). Should have exactly 3 input channels,
            and width and height should be no smaller than 32.
            Read more: https://keras.io/api/applications/vgg,  by default (240, 240, 3)
        """
        self.input_shape = input_shape
        self.model = VGG16(weights=weights, include_top=False, input_shape=input_shape)

    def extract_features(self, images: List[np.ndarray]) -> List[np.ndarray]:
        """Extracts and flattens VGG16 embeddings from the provided images

        Parameters
        ----------
        images : List[np.ndarray]
            List of images to extract features from

        Returns
        -------
        List[np.ndarray]
            Extracted features
        """
        width = self.input_shape[1]
        height = self.input_shape[0]
        prepared = [cv2.resize(image, (width, height)) for image in images]
        prepared = preprocess_input(np.array(prepared))
        vgg16_features = self.model.predict(prepared)
        vgg16_flat = vgg16_features.reshape(len(vgg16_features), -1)
        return vgg16_flat
