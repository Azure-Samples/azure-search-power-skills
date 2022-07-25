from __future__ import annotations

import json
import logging
import os

import azure.functions as func
import jsonschema
from azure.ai.textanalytics import (
    TextAnalyticsClient,
    RecognizeCustomEntitiesAction,
    TextDocumentInput, AnalyzeActionsLROPoller, RecognizeCustomEntitiesResult, DocumentError
)
from azure.core.credentials import AzureKeyCredential
from azure.core.exceptions import HttpResponseError


def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')

    endpoint = os.environ["TA_ENDPOINT"]
    key = os.environ["TA_KEY"]
    project_name = os.environ["PROJECT_NAME"]
    deployment_name = os.environ["DEPLOYMENT_NAME"]

    try:
        try:
            body = req.get_json()
        except ValueError as e:
            logging.debug(e)
            return func.HttpResponse(
                "Body is not valid JSON.",
                status_code=400
            )

        jsonschema.validate(instance=body, schema=requestSchema)
        values = [
            TextDocumentInput(id=v["recordId"], text=v["data"]["text"], language=v["data"].get("lang", "en"))
            for v in body["values"]
        ]
    
        text_analytics_client = TextAnalyticsClient(endpoint, AzureKeyCredential(key))

        poller: AnalyzeActionsLROPoller = text_analytics_client.begin_analyze_actions(
            values,
            [RecognizeCustomEntitiesAction(project_name=project_name, deployment_name=deployment_name)]
        )

        formatted_results = []

        res = poller.result()

        for page in res:
            for custom_entities_result in page:
                custom_entities_result: RecognizeCustomEntitiesResult | DocumentError
                if not custom_entities_result.is_error:
                    custom_entities_result: RecognizeCustomEntitiesResult
                    formatted_results.append({
                        "recordId": custom_entities_result.id,
                        "data": {
                            "entities": custom_entities_result.entities
                        },
                        "warnings": [{"message": w.message} for w in custom_entities_result.warnings]
                    })
                else:
                    custom_entities_result: DocumentError
                    formatted_results.append({
                        "recordId": custom_entities_result.id,
                        "data": {},
                        "errors": [{"message": custom_entities_result.error.message}]
                    })

        return func.HttpResponse(json.dumps({"values": formatted_results}, default=vars), mimetype="application/json")

    except jsonschema.exceptions.ValidationError as e:
        return func.HttpResponse(
            "Invalid request: {0}".format(e),
            status_code=400
        )
    except jsonschema.exceptions.SchemaError as e:
        logging.critical("FATAL SCHEMA ERROR: {0}".format(e))
        return func.HttpResponse(
            "Internal schema Error, please contact the the maintainer of this endpoint.",
            status_code=500
        )
    except HttpResponseError as e:
        logging.info(e)
        if e.status_code == 400:
            return func.HttpResponse(
                "Received 400 response from Language services:\n{0}".format(e.message),
                status_code=400
            )
        else:
            return func.HttpResponse(
                "Error occured while contacting the language services.",
                status_code=500
            )
    except Exception as e:
        logging.info(e)
        return func.HttpResponse(
            "Unknown error occurred.",
            status_code=500
        )


# TODO: check language services limits (max batch size, etc)
requestSchema = {
    "$schema": "http://json-schema.org/draft-04/schema#",
    "type": "object",
    "properties": {
        "values": {
            "type": "array",
            "minItems": 1,
            "items": {
                "type": "object",
                "properties": {
                    "recordId": {
                        "type": "string"
                    },
                    "data": {
                        "type": "object",
                        "properties": {
                            "text": {
                                "type": "string",
                                "minLength": 1
                            },
                            "lang": {
                                "type": "string"
                            }
                        },
                        "required": [
                            "text"
                        ]
                    }
                },
                "required": [
                    "recordId",
                    "data"
                ]
            }
        }
    },
    "required": [
        "values"
    ]
}
