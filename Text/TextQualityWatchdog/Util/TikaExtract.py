###
# This script is used to crawl a source directory for unstructured files (PDFs, PPTs, etc.),
# send requests to the Tika server, and save the extracted text files in a destination directory.
# Currently, non-English files are filtered out.
#
# Please note that the original use case for this helper script was to extract text files from a
# static dataset, which is why we use the "last read file" to keep track of progress. If files are
# updated, this script should be re-run to re-process them.
###

import os
import io
import pickle

from tika import parser
from tika import language
from azure.storage import blob
from azure.storage.blob import BlobServiceClient, BlobClient, ContainerClient, __version__

print("Azure Blob Storage v" + __version__ + " - Tika extraction handler\n")

VERBOSE = False
PARSER_MAX_RETRIES = 3

# Root path of this script
APP_ROOT = os.path.dirname(os.path.abspath(__file__))
APP_ROOT_STR = APP_ROOT.__str__()

# Destination directory to save extracted text files to
DEST_DIR = 'ExtractedTextData'

# Download directory that we save unstructured files from blob storage to
DOWNLOAD_DIR = 'CommonCrawlData'

# Metadata directory that we save the tika-eval metrics to in a pickle file
METADATA_DIR = 'Metadata'

# Metrics directory where we save metrics used to summarize the run
METRICS_DIR = 'Metrics'

# Retrieve the connection string for use with the application. The storage
# connection string is stored in an environment variable on the machine
# running the application called AZURE_STORAGE_CONNECTION_STRING.
connect_str = os.getenv('AZURE_STORAGE_CONNECTION_STRING')

# Create the BlobServiceClient object which will be used to create a container client
blob_service_client = BlobServiceClient.from_connection_string(connect_str)

# Get the container client corresponding to the commoncrawl container
cont_client = blob_service_client.get_container_client('commoncrawl3-refetched')

# Get the list of all the blobs inside the commoncrawl container
blob_list = cont_client.list_blobs()

# Read the high water mark
last_read_file_name = None
last_read_file_path = os.path.join(APP_ROOT_STR, METRICS_DIR, "lastread.txt")
if os.path.exists(last_read_file_path):
    with open(last_read_file_path, "r") as last_read_file:
        last_read_file_name = last_read_file.readline()

# Read the metrics dictionary, or create it if it does not exist
metrics_dict = None
metrics_dict_file_path = os.path.join(APP_ROOT_STR, METRICS_DIR, "dataset_metrics.pkl")
if os.path.exists(metrics_dict_file_path):
    with open(metrics_dict_file_path, "rb") as metrics_dict_file:
        metrics_dict = pickle.load(metrics_dict_file)
else:
    metrics_dict = {
        "success" : 0, # English files that had text successfully extracted
        "nonEng" : 0, # Files that were not in English
        "failed" : 0, # Files that failed to have text extracted
        "total" : 0 # Total count of the files
    }

skipped_file_count = 0

# Process each blob
for cur_blob in blob_list:
    try:
        # Get the name of the current blob
        blobname = cur_blob.name
        filename = blobname.split('/')[1]

        # Iterate until we hit the high water mark
        if last_read_file_name != None and filename < last_read_file_name:
            skipped_file_count += 1

            if VERBOSE:
                print(blobname)
                print("skip!")

            continue

        if last_read_file_name != None and filename == last_read_file_name:
            print(f"Skipped {skipped_file_count} files.")

        print(blobname)

        # Get the blob client for the current blob
        blob_client = cont_client.get_blob_client(blobname)

        # Get the path to save the unstructured file to
        download_file_path = os.path.join(APP_ROOT_STR, DOWNLOAD_DIR, filename)

        # Get the full path to the destination file
        extracted_file_path = os.path.join(APP_ROOT_STR, DEST_DIR, filename)

        # Get the full path to the metadata file
        metadata_file_path = os.path.join(APP_ROOT_STR, METADATA_DIR, filename)

        # Download the blob to a local file
        with open(download_file_path, "wb") as download_file:
            download_file.write(blob_client.download_blob().readall())
        print("Downloaded blob to \n\t" + download_file_path)

        # Invoke tika server to perform extraction
        parsedFile = None

        for attempt_num in range(1, PARSER_MAX_RETRIES + 1):
            try:
                parsedFile = parser.from_file(download_file_path)
            except Exception as e:
                print(f"Exception on attempt {attempt_num} to parse file: ", e)
            else:
                break

        # Skip file if fail to extract text
        if (parsedFile is None) or (parsedFile['content'] is None) or (type(parsedFile['content']) != str):
            os.remove(download_file_path)
            print("No text extracted, file skipped.\n")

            # Update high water mark
            with open(last_read_file_path, "w") as last_read_file:
                last_read_file.write(filename)

            # Update metrics and save to disk
            metrics_dict['total'] += 1
            metrics_dict['failed'] += 1
            with open(metrics_dict_file_path, 'wb') as picklefile:
                pickle.dump(metrics_dict, picklefile)

            continue

        # Write the extracted text to the destination
        with io.open(extracted_file_path, "w", encoding="utf-8") as f:
            f.write(parsedFile['content'])

        # Invoke tika to detect language of the text file
        lang = language.from_file(extracted_file_path)

        # Remove non-English text files
        if lang != "en":
            os.remove(extracted_file_path)
            os.remove(download_file_path)
            print("Non-English text file removed.\n")

            # Update metrics and save to disk
            metrics_dict['total'] += 1
            metrics_dict['nonEng'] += 1
            with open(metrics_dict_file_path, 'wb') as picklefile:
                pickle.dump(metrics_dict, picklefile)

        else:
            # Serialize metadata and dump in pickle file
            metadata_dict = parsedFile['metadata']
            with open(metadata_file_path + '.pkl', 'wb') as picklefile:
                pickle.dump(metadata_dict, picklefile)

            # Update metrics and save to disk
            metrics_dict['total'] += 1
            metrics_dict['success'] += 1
            with open(metrics_dict_file_path, 'wb') as picklefile:
                pickle.dump(metrics_dict, picklefile)

            print("Success!\n")

        # Update high water mark
        with open(last_read_file_path, "w") as last_read_file:
            last_read_file.write(filename)

    except Exception as ex:
        print('Exception:')
        print(ex)
        print()