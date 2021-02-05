"""
This module contains image similarity detector class
"""
from typing import List
import logging

import numpy as np

from sklearn.base import ClassifierMixin
from ..extractors.extractor import Extractor

class ImageSimilarityDetector():

    def __init__(self, extractor: Extractor, model: ClassifierMixin):
        """Creates an instance of `ImageSimilarityDetector` class

        Parameters
        ----------
        extractor : Extractor
            Extractor class to use for feature extraction
        model : ClassifierMixin
            A *clustering* model that contains `train` and `predict` class
        """
        self.model = model
        self.extractor = extractor

    def train(self, X: List[np.ndarray]) -> np.ndarray:
        """Trains a clustering model and returns labels for the training set

        Parameters
        ----------
        X : List[np.ndarray]
            Images to use for clustering

        Returns
        -------
        np.ndarray
            Predicted labels
        """
        X_prep = self.extractor.extract_features(X)
        labels = self.model.fit_predict(X_prep)
        n_clusters_ = len(np.unique(labels)) - (1 if -1 in labels else 0)
        n_noise_ = list(labels).count(-1)
        
        logging.info('Estimated number of clusters: %d', n_clusters_)
        logging.info('Estimated number of noise points: %d', n_noise_)
        return labels

    def assign_group(self, X: List[np.ndarray]) -> np.ndarray:
        """Approximates a group that the points belong to

        Parameters
        ----------
        X : List[np.ndarray]
            Images to assign labels to

        Returns
        -------
        np.ndarray
            Predicted labels
        """
        X_prep = self.extractor.extract_features(X)
        return self.model.predict(X_prep)
