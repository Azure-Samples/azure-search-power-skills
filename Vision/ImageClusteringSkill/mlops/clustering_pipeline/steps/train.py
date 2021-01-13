"""
Training step for clustering-based image similarity detector
"""
import logging
import tempfile
import os
import argparse
from os.path import join
import random
import joblib
import numpy as np

from azureml.core import Run

from ml.extractors.vgg16_extractor import VGG16Extractor
from ml.models.DBSCAN import DBSCANv2
from ml.similarity.detector import ImageSimilarityDetector
from ml.utils.image import read_images_from_list, show_images

logging.basicConfig(level=logging.INFO)
log: logging.Logger = logging.getLogger(__name__)

def main():
    """Main function for receiving args, and passing them through to score Custom Vision"""
    parser = argparse.ArgumentParser()
    parser.add_argument('--input_dir', help='Path to the folder containing the training dataset', required=True)
    parser.add_argument('--fraction', help='Fraction of images to use for training', required=True, type=float)
    parser.add_argument('--recursive', help='Indicates whether to go recursively into subdirectories', required=True, type=bool)
    parser.add_argument(
        '--eps',
        help="""Eps parameter for DBSCAN. The maximum distance between two samples for one
        to be considered as in the neighborhood of the other.""",
        default=0.5,
        type=float
    )
    parser.add_argument(
        '--min_samples',
        help="""Min samples parameter for dbscan. The number of samples (or total weight) in
        a neighborhood for a point to be considered as a core point.""",
        default=5,
        type=int
    )
    parser.add_argument(
        '--metric',
        help="The metric to use when calculating distance between instances in a feature array",
        default="cosine"
    )
    parser.add_argument(
        '--report_n_samples',
        help="Number of samples to display in the report (for each cluster)",
        default=5,
        type=int
    )
        
    parser.add_argument('--model_name', required=True)
    args = parser.parse_args()

    # Setup extractor, model and similarity detector 
    log.info("Creating image similarity model...")
    extractor = VGG16Extractor()
    model = DBSCANv2(eps=args.eps, min_samples=args.min_samples, metric=args.metric)
    detector = ImageSimilarityDetector(extractor, model)

    # List the input folder
    log.info("Reading images from %s...", args.input_dir)
    files = []

    for r, _, f in os.walk(args.input_dir):
        for file in f:
            files.append(join(r, file))
        if not args.recursive:
            break
    # Randomly select fraction of images for training
    files = random.sample(files, int(len(files) * args.fraction))
    # Read images
    images, _ = read_images_from_list(files)

    # Training model
    log.info("Training similarity model...")
    labels = detector.train(images)

    # Saving model
    path = 'outputs'
    log.info("Training completed. Registering model...")
    file_name = f'{args.model_name}.pkl'
    tags = {
        "min_samples": args.min_samples,
        "eps": args.eps,
        "metric": args.metric
    }
    joblib.dump(value=detector.model, filename=os.path.join(path, file_name))
    
    # Saving clustering report
    os.makedirs(os.path.join(path, 'report'))
    cluster_ids = np.unique(labels)
    for cluster_id in cluster_ids:
        sub_images = images[labels == cluster_id]
        cluster = np.random.choice(sub_images, min(args.report_n_samples, len(sub_images)), replace=False)
        cluster_labels = [cluster_id] * len(cluster)
        fig = show_images(cluster, cols=4, titles=cluster_labels)
        fig.savefig(os.path.join(path, 'report', f'cluster_{cluster_id}_samples.png'))
  
    # Registering everything as a model
    run = Run.get_context()
    run.upload_folder(path, path)
    run.register_model(model_name=args.model_name, model_path=path, tags=tags)
    log.info("Model registered. Experiment completed.")


if __name__ == "__main__":
    main()
