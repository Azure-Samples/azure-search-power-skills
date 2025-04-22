# sample_request.py
import base64
import logging
import os
import json
import requests
from dotenv import find_dotenv, load_dotenv

# --- Script Configuration ---
# Ensure the image file is in the same directory as this script,
# or update the path accordingly.
IMAGE_FILE_PATH = "thrive35.png"
# Set to True to request captions, False for OCR only
REQUEST_CAPTIONS = True

# --- Logging Setup ---
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# --- Load Environment Variables ---
# Create a '.env' file in this directory with:
# FUNCTION_ENDPOINT=https://<your-function-app-name>.azurewebsites.net
# FUNCTION_KEY=<your-function-host-key>
load_dotenv(find_dotenv())
FUNCTION_ENDPOINT = os.getenv("FUNCTION_ENDPOINT")
FUNCTION_KEY = os.getenv("FUNCTION_KEY")

# --- Input Validation ---
if not FUNCTION_ENDPOINT or not FUNCTION_KEY:
    logger.error(
        "Error: FUNCTION_ENDPOINT and FUNCTION_KEY must be set in the .env file."
    )
    exit(1)
if not os.path.exists(IMAGE_FILE_PATH):
    logger.error(f"Error: Image file not found at '{IMAGE_FILE_PATH}'")
    exit(1)
logger.info(f"Target Endpoint: {FUNCTION_ENDPOINT}")
logger.info(
    f"Using Function Key: {'Yes' if FUNCTION_KEY else 'No'}"
)  # Avoid logging the key itself

# --- Prepare Request ---
api_url = f"{FUNCTION_ENDPOINT}/api/aivisionapiv4"
params = {"code": FUNCTION_KEY, "use_caption": str(REQUEST_CAPTIONS).lower()}

# Convert image to base64
try:
    with open(IMAGE_FILE_PATH, "rb") as image_file:
        image_base64 = base64.b64encode(image_file.read()).decode("utf-8")
    logger.info(f"Image '{IMAGE_FILE_PATH}' encoded to base64.")
except Exception as e:
    logger.error(f"Error encoding image: {e}")
    exit(1)

# Construct JSON payload
request_data = {
    "values": [
        {
            "recordId": f"record-{os.path.basename(IMAGE_FILE_PATH)}",  # Use filename in record ID
            "data": {
                "image": image_base64,
                "languageCode": "en",  # Example language code
            },
        }
    ]
}
headers = {"Content-Type": "application/json"}

# --- Send Request ---
logger.info(
    f"Sending POST request to: {api_url} (Caption requested: {REQUEST_CAPTIONS})"
)
try:
    response = requests.post(
        api_url,
        json=request_data,
        headers=headers,
        params=params,
        timeout=90,  # Increased timeout slightly
    )
    response.raise_for_status()  # Raises HTTPError for 4xx/5xx status codes

    logger.info(f"Request successful! Status Code: {response.status_code}")
    try:
        response_json = response.json()
        logger.info("Response JSON:")
        print(json.dumps(response_json, indent=2))  # Pretty print
    except json.JSONDecodeError:
        logger.warning("Response was not valid JSON.")
        logger.info(f"Response Text: {response.text}")

# --- Error Handling ---
except requests.exceptions.Timeout:
    logger.error("Error: The request timed out.")
except requests.exceptions.HTTPError as e:
    logger.error(f"Error: HTTP Error occurred: {e}")
    logger.error(f"Status Code: {e.response.status_code}")
    try:
        error_body = e.response.json()
        logger.error(f"Error Response Body: {json.dumps(error_body, indent=2)}")
    except json.JSONDecodeError:
        logger.error(f"Error Response Text: {e.response.text}")
except requests.exceptions.RequestException as e:
    logger.error(f"Error: An unexpected error occurred: {e}")
    exit(1)
except Exception as e:
    logger.error(f"An unexpected error occurred: {e}", exc_info=True)
    exit(1)
