###
# This script is used to preprocess text files located in a source directory
# and save the cleaned text files in a destination directory as a tsv
###

# Standard libraries
import os
import pickle
import pandas as pd

# Helper scripts
from ..PreprocessData import normalize_text

APP_ROOT = os.path.dirname(os.path.abspath(__file__))
APP_ROOT_STR = APP_ROOT.__str__()

# Source directory containing all of the text files extracted from tika
SOURCE_DIR = 'ExtractedTextData'

# Destination directory to save the final tsv file to
DEST_DIR = 'Data'

# Metadata directory that we saved the tika-eval metrics to in a pickle file
METADATA_DIR = 'Metadata'

text_list = []  # list to hold the text extracted from each document
score_list = []  # the scores for each text
filename_list = []  # the filename for each text

source_path = os.path.join(APP_ROOT_STR, SOURCE_DIR)

for root, dirs, files in os.walk(source_path, topdown=False):
    for name in files:
        print(name)

        # Get the full path to the metadata file
        metadata_file_path = os.path.join(APP_ROOT_STR, METADATA_DIR, name + ".pkl")

        # Get the tika-eval oov score from the metadata dictionary
        try:
            with open(metadata_file_path, 'rb') as pkl_file:
                meta_dict = pickle.load(pkl_file)
                if type(meta_dict['tika-eval:oov']) == list:
                    oov_score = float(meta_dict['tika-eval:oov'][0])
                else:
                    oov_score = float(meta_dict['tika-eval:oov'])
        except FileNotFoundError as e:
            print(e)
            print()
            continue
        
        print(oov_score)

        # Only record the filename, score, and metadata if the OOV score is valid
        if oov_score < 0:
            print('Tika OOV Score less than zero!')
            print()
            continue

        filename_list.append(name)
        score_list.append(oov_score)

        # Apply the text normalization and save the processed text
        filepath = os.path.join(root, name)
        with open(filepath, 'r', encoding='utf-8') as f:
            lines = f.read()
            normalized_lines = normalize_text(lines)
            text_list.append(normalized_lines)

        print()

dataset_dict = {
    'text' : text_list,
    'tika_oov_score': score_list,
    'filename' : filename_list
}

# Create a dataframe using the dataset dictionary
tika_text_df = pd.DataFrame(data=dataset_dict)

# Get the destination path to which the csv should be saved to
dest_file_path = os.path.join(APP_ROOT_STR, DEST_DIR, "tika_text_dataset.tsv")

# Save the dataset as a tsv file
tika_text_df.to_csv(dest_file_path, sep = '\t', index = False)
