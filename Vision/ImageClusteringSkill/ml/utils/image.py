"""
Image preprocessing module
"""
from typing import Tuple, List

import os
from os.path import join
import logging

import cv2
import numpy as np
from tqdm import tqdm
import matplotlib.pyplot as plt


def read_rgb_image(file_path: str) -> np.ndarray:
    """Reads an RGB image from the specified path

    Parameters
    ----------
    file_path : str
        Path to the image file

    Returns
    -------
    np.ndarray
        Read image

    Raises
    ------
    ValueError
        Raise if the expension of the file doesn't correspond to jpeg, jpg or png
    """
    if file_path.split(".")[-1].lower() not in {"jpeg", "jpg", "png"}:
        raise ValueError("The extension of the file should be jpeg, jpg or png")
    image = cv2.imread(file_path)
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    return image


def read_images_from_folder(folder_path: str) -> Tuple[np.ndarray, List[str]]:
    """Reads images from the specified folder

    Parameters
    ----------
    folder_path : str
        Path to the folder to read images from

    Returns
    -------
    Tuple[np.ndarray, List[str]]
        Array of images and list of files' names
    """
    images = []
    file_names = []
    for file_name in tqdm(os.listdir(folder_path)):
        try:
            image = read_rgb_image(join(folder_path, file_name))
        except ValueError as e:
            logging.error("Couldn't read ' %s'. %s. Skipped.", image, e)
        else:
            images.append(image)
            file_names.append(file_name)
    return np.array(images), file_names


def read_images_from_list(image_list: list) -> Tuple[np.ndarray, List[str]]:
    """Reads images from the specified folder

    Parameters
    ----------
    image_list : list
        list of images paths

    Returns
    -------
    Tuple[np.ndarray, List[str]]
        Array of images and list of files' names
    """
    images = []
    file_names = []
    for file in tqdm(image_list):
        try:
            file_name = file.split('\\')[-1]
            image = read_rgb_image(file)
        except ValueError as e:
            logging.error("Couldn't read ' %s'. %s. Skipped.", image, e)
        else:
            images.append(image)
            file_names.append(file_name)
    return np.array(images), file_names


"""
This module contains functions for image visualization
"""


def show_images(images: List[np.ndarray], cols: int = 1, titles: List[str] = None, max_width: int = 16):
    """Displays array of images with specified number of columns

    Parameters
    ----------
    images : List[np.ndarray]
        List of images to be displayed
    cols : int, optional
        Number of columns to be used for visualization, by default 1
    titles : List[str], optional
        List of titles to be assigned to images, by default None
    max_width : int, optional
        Maximum width of the resulting plot (inches), by default 16

    Raises
    ------
    ValueError
        Raised if the provided images array length doesn't correspond the length of the titles array
    """
    if (titles is not None) and (len(images) != len(titles)):
        raise ValueError("Images array and titles array must be of the same length")
    n_images = len(images)
    if titles is None:
        titles = [f"Image {i}" for i in range(1, n_images + 1)]
    rows = int(np.ceil(n_images / float(cols)))
    fig = plt.figure()
    for i, (image, title) in enumerate(zip(images, titles)):
        ax = fig.add_subplot(rows, cols, i + 1)  # pylint: disable=C0103
        if image.ndim == 2:
            plt.gray()
        plt.imshow(image)
        ax.set_title(title)
        plt.xticks([]), plt.yticks([])  # pylint: disable=W0106
    fig.set_size_inches(max_width, max_width / (cols + 1) * rows)
    plt.tight_layout()
    plt.show()
    return fig

