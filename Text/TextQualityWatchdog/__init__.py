# Standard libraries
import os
import json
import logging
from typing import Text

# Azure functions
import azure.functions as func

# Inference runtime
import onnxruntime as ort
from tokenizers import BertWordPieceTokenizer

# Helper scripts
from .PreprocessData import normalize_text, truncate_text
from .Predict import get_ids_and_masks, predict

# Initialize ONNX runtime and language model tokenizer
vocab_file_path = os.path.join(os.path.dirname(__file__), "Model/bert-base-uncased-vocab.txt")
onnx_file_path = os.path.join(os.path.dirname(__file__), "Model/watchdog_model.onnx")

tokenizer = BertWordPieceTokenizer(vocab_file_path)
tokenizer.enable_padding(pad_id=0, pad_token="[PAD]", length=128)
tokenizer.enable_truncation(max_length=128)

ort_session = ort.InferenceSession(onnx_file_path)

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Invoked TextQualityWatchdog Skill.')

    try:
        body = json.dumps(req.get_json())

        if body:
            logging.info(body)
            values = json.loads(body)['values']
            results = {}
            results["values"] = []

            for value in values:
                text = value['data']['text']

                # Apply puntuation and whitespace normalization, and convert to lowercase
                text = normalize_text(text)

                # Truncate the text to a maximum of 128 (default) whitespace separated tokens
                text = truncate_text(text)

                # Compute the input tokens and attention masks for the text sequence
                input_ids, attention_masks = get_ids_and_masks(tokenizer, text)

                # Call the ONNX model to perform inference on the input
                flat_prediction = predict(ort_session, input_ids, attention_masks)

                payload = (
                              {
                                  "recordId": value['recordId'],   
                                  "data": {
                                      "text_quality_warning": int(flat_prediction[0])
                                  }
                              }
                          )

                results["values"].append(payload)

            result = json.dumps(results, ensure_ascii=False)

            return func.HttpResponse(result, mimetype="application/json")

        else:
            return func.HttpResponse(
                "Invalid body",
                status_code=400
            )

    except ValueError:
        return func.HttpResponse(
             "Invalid body",
             status_code=400
        )
