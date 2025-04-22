import json
import logging
import os
import base64

import azure.functions as func
from azure.ai.vision.imageanalysis import ImageAnalysisClient
from azure.ai.vision.imageanalysis.models import VisualFeatures
from azure.core.exceptions import AzureError, HttpResponseError
from azure.identity import ManagedIdentityCredential

app = func.FunctionApp()

AI_VISION_ENDPOINT = os.getenv("AI_VISION_ENDPOINT")

ai_vision_client = None
client_initialized = False


def get_ai_vision_client():
    """Helper to initialize or get the existing client."""
    global ai_vision_client, client_initialized

    if not client_initialized:
        try:
            logging.info(
                f"Attempting to initialize AI Vision client with endpoint: {AI_VISION_ENDPOINT}"
            )
            credential = ManagedIdentityCredential()
            logging.info("Using Managed Identity for authentication.")
            if not AI_VISION_ENDPOINT:
                raise ValueError("AI_VISION_ENDPOINT environment variable is not set.")
            ai_vision_client = ImageAnalysisClient(
                endpoint=AI_VISION_ENDPOINT,
                credential=credential,
            )
            client_initialized = True
            logging.info("AI Vision client initialized successfully.")
        except Exception as e:
            logging.error(f"Failed to initialize AI Vision client: {e}", exc_info=True)
            ai_vision_client = None
            client_initialized = False
    return ai_vision_client


@app.route(route="aivisionapiv4", auth_level=func.AuthLevel.FUNCTION)
def aivisionapiv4(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Function 'aivisionapiv4' invoked.")

    client = get_ai_vision_client()
    if not client:
        logging.error("AI Vision client is not available.")
        return func.HttpResponse(
            json.dumps({"error": "AI Vision client could not be initialized."}),
            status_code=500,
            mimetype="application/json",
        )

    try:
        req_body = req.get_json()
    except ValueError:
        logging.error("Invalid JSON in request.", exc_info=True)
        return func.HttpResponse(
            json.dumps({"error": "Invalid JSON in request."}),
            status_code=400,
            mimetype="application/json",
        )

    use_caption = req.params.get("use_caption", "false").lower() == "true"
    default_language = req.params.get("default_language", "en")
    logging.info(f"Caption processing requested: {use_caption}")
    logging.info(f"Default language set to: {default_language}")

    response_values = []

    values_data = req_body.get("values", [])

    for record in values_data:

        record_id = record.get("recordId")
        record_data = {}
        record_errors = []
        record_warnings = []

        logging.info(f"Processing record ID: {record_id}")

        record_input_data_raw = record.get("data", {})

        if isinstance(record_input_data_raw, dict):
            record_input_data = record_input_data_raw
        else:
            logging.error(
                f"Invalid type for 'data' field in record ID {record_id}. Expected object/dict, got {type(record_input_data_raw).__name__}. Skipping image processing for this record."
            )
            record_errors.append(
                {"message": "Invalid format for 'data' field. Expected an object."}
            )
            record_input_data = {}
            image_base64 = None

        if isinstance(record_input_data_raw, dict):
            image_base64 = record_input_data.get("image")
            language_code = record_input_data.get("languageCode", default_language)
            if not isinstance(language_code, str) or not language_code.strip():
                logging.warning(
                    f"Invalid or missing languageCode for record {record_id}, using default '{default_language}'."
                )
                language_code = default_language
            else:
                language_code = language_code.strip().lower()
        else:
            language_code = default_language

        logging.info(f"Using language '{language_code}' for record ID: {record_id}")

        if not image_base64:
            if not any(
                err["message"] == "Invalid format for 'data' field. Expected an object."
                for err in record_errors
            ):
                error_msg = f"No image data provided or 'data' field was invalid for record ID: {record_id}."
                logging.error(error_msg)
                record_errors.append(
                    {"message": "Missing or invalid image data input."}
                )

            response_values.append(
                {
                    "recordId": record_id,
                    "data": {},
                    "errors": record_errors,
                    "warnings": record_warnings,
                }
            )
            continue

        try:
            image_bytes = base64.b64decode(image_base64)
            current_ocr_text = ""
            current_caption = ""

            visual_features = [VisualFeatures.READ]
            if use_caption:
                visual_features.append(VisualFeatures.CAPTION)

            result = client.analyze(
                image_data=image_bytes,
                visual_features=visual_features,
                language=language_code,
            )

            if result.read and result.read.blocks:
                document_contents = [
                    line.text for block in result.read.blocks for line in block.lines
                ]
                current_ocr_text = " ".join(document_contents)
            else:
                logging.warning(f"No read results for record ID: {record_id}.")

            if use_caption:
                if result.caption:
                    current_caption = result.caption.text
                    logging.info(
                        f"Caption for {record_id}: '{current_caption}', Confidence: {result.caption.confidence:.4f}"
                    )
                else:
                    logging.warning(
                        f"No caption result returned (language: {language_code}) for record ID: {record_id}."
                    )

            record_data["image_text"] = current_ocr_text
            if use_caption:
                record_data["caption"] = current_caption

            response_values.append(
                {
                    "recordId": record_id,
                    "data": record_data,
                    "errors": record_errors,
                    "warnings": record_warnings,
                }
            )

        except (AzureError, HttpResponseError) as e:
            error_message = str(e)
            if "NotSupportedLanguage" in error_message or (
                hasattr(e, "error")
                and e.error
                and e.error.code == "NotSupportedLanguage"
            ):
                error_msg = f"Language '{language_code}' not supported by AI Vision for the requested features for record ID {record_id}. Error: {e}"
                logging.error(error_msg)
                record_errors.append(
                    {
                        "message": f"Language '{language_code}' not supported for requested features."
                    }
                )
            else:
                error_msg = f"Azure SDK Error processing record ID {record_id}: {e}"
                logging.error(error_msg, exc_info=True)
                record_errors.append(
                    {"message": f"An Azure service error occurred. Details: {str(e)}"}
                )

            response_values.append(
                {
                    "recordId": record_id,
                    "data": {},
                    "errors": record_errors,
                    "warnings": record_warnings,
                }
            )

        except Exception as e:
            error_msg = f"Unexpected error processing record ID {record_id}: {e}"
            logging.error(error_msg, exc_info=True)
            record_errors.append(
                {"message": f"An unexpected error occurred. Details: {str(e)}"}
            )
            response_values.append(
                {
                    "recordId": record_id,
                    "data": {},
                    "errors": record_errors,
                    "warnings": record_warnings,
                }
            )

    return func.HttpResponse(
        json.dumps({"values": response_values}),
        status_code=200,
        mimetype="application/json",
    )
