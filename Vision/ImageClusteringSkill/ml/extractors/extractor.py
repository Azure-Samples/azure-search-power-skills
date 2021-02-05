"""
This module contains `Extractor` abstract class. Typically the classes inherited from this one are used to extract features
that can be used for multiple purposes, e.g.: image similarity calculation
"""
from typing import Tuple, List
from abc import ABC, abstractmethod

import numpy as np

class Extractor(ABC):

    @abstractmethod
    def extract_features(self, images: List[np.ndarray]) -> List[np.ndarray]:
        """Extracts features from the provided array of images

        Parameters
        ----------
        images : List[np.ndarray]
            List of images to extract features from

        Returns
        -------
        List[np.ndarray]
            Extracted features
        """
        pass