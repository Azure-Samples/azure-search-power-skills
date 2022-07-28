from __future__ import annotations

import json
import logging
import os

import azure.functions as func
import jsonschema
from azure.ai.textanalytics import (
    TextAnalyticsClient,
    MultiCategoryClassifyAction,
    SingleCategoryClassifyAction,
    SingleCategoryClassifyResult,
    TextDocumentInput,
    AnalyzeActionsLROPoller,
    MultiCategoryClassifyResult,
    DocumentError,
)
from azure.core.credentials import AzureKeyCredential
from azure.core.exceptions import HttpResponseError


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info("Python HTTP trigger function processed a request.")

    endpoint = os.environ["TA_ENDPOINT"]
    key = os.environ["TA_KEY"]
    project_name = os.environ["PROJECT_NAME"]
    deployment_name = os.environ["DEPLOYMENT_NAME"]
    service_type = os.environ["CLASSIFICATION_TYPE"]

    if service_type != "multi" and service_type != "single":
        return func.HttpResponse(
            "The function is not configured correctly. The environment variable CLASSIFICATION_TYPE accepts either 'multi' or 'single'",
            status_code=500,
        )

    try:
        body = req.get_json()
    except ValueError as e:
        logging.info(e)
        return func.HttpResponse("Body is not valid JSON.", status_code=400)

    try:
        jsonschema.validate(instance=body, schema=get_request_schema())
    except jsonschema.exceptions.ValidationError as e:
        return func.HttpResponse("Invalid request: {0}".format(e), status_code=400)

    try:
        values = map_dict_to_text_input(body)

        text_analytics_client = TextAnalyticsClient(endpoint, AzureKeyCredential(key))

        poller: AnalyzeActionsLROPoller = text_analytics_client.begin_analyze_actions(
            values,
            [
                MultiCategoryClassifyAction(
                    project_name=project_name, deployment_name=deployment_name
                )
            ]
            if service_type == "multi"
            else [
                SingleCategoryClassifyAction(
                    project_name=project_name, deployment_name=deployment_name
                )
            ],
        )

        res = poller.result()

        return func.HttpResponse(
            result_to_json(service_type, res), mimetype="application/json"
        )
    except HttpResponseError as e:
        logging.info(e)
        return func.HttpResponse(
            "Received {0} response from Language services:\n{1}".format(
                e.status_code, e.message
            ),
            status_code=400,
        )


def result_to_json(service_type, res):
    formatted_results = []

    for page in res:
        for result in page:
            result: MultiCategoryClassifyResult | SingleCategoryClassifyResult | DocumentError
            if not result.is_error:
                result: MultiCategoryClassifyResult | SingleCategoryClassifyResult
                formatted_results.append(
                    {
                        "recordId": result.id,
                        "data": {
                            "class": result.classifications
                            if service_type == "multi"
                            else [result.classification]
                        },
                        "warnings": [{"message": w.message} for w in result.warnings],
                    }
                )
            else:
                result: DocumentError
                formatted_results.append(
                    {
                        "recordId": result.id,
                        "data": {},
                        "errors": [{"message": result.error.message}],
                    }
                )

    return json.dumps({"values": formatted_results}, default=vars)


def map_dict_to_text_input(body):
    return [
        TextDocumentInput(
            id=v["recordId"],
            text=v["data"]["text"],
            language=v["data"].get("lang", "en"),
        )
        for v in body["values"]
    ]


def get_request_schema():
    return {
        "$schema": "http://json-schema.org/draft-04/schema#",
        "type": "object",
        "properties": {
            "values": {
                "type": "array",
                "minItems": 1,
                "items": {
                    "type": "object",
                    "properties": {
                        "recordId": {"type": "string"},
                        "data": {
                            "type": "object",
                            "properties": {
                                "text": {"type": "string", "minLength": 1},
                                "lang": {"type": "string"},
                            },
                            "required": ["text"],
                        },
                    },
                    "required": ["recordId", "data"],
                },
            }
        },
        "required": ["values"],
    }
