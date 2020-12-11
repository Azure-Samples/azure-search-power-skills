"""
This module contains a sklearn.cluster.DBSCAN wrapper that contains distance-based `predict` method
"""
import numpy as np
from sklearn.utils.validation import check_array, check_is_fitted
from sklearn.base import BaseEstimator, ClassifierMixin
from sklearn.cluster import DBSCAN
from scipy.spatial import distance


class DBSCANv2(DBSCAN, BaseEstimator, ClassifierMixin):

    def predict(self, X: np.ndarray) -> np.ndarray:
        """Predicts which class the provided examples belong to based on the distance metrics

        Parameters
        ----------
        X : np.ndarray
            Array of points to predict cluster labels for

        Returns
        -------
        np.ndarray
            Predicted labels
        """
        check_is_fitted(self)
        X = check_array(X)
        y_new = np.ones(shape=len(X), dtype=int)*-1
        for j, x_new in enumerate(X):
            for i, x_core in enumerate(self.components_):
                dist = distance.cdist(
                    x_new.reshape(1, -1), 
                    x_core.reshape(1, -1),
                    metric=self.metric
                )[0][0]
                if dist < self.eps:
                    y_new[j] = self.labels_[self.core_sample_indices_[i]]
                    break
        return y_new
