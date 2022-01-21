import logging
import json
import os
import logging
import datetime, time
from json import JSONEncoder
import os
from azure.core.exceptions import ResourceNotFoundError
from azure.ai.formrecognizer import DocumentAnalysisClient
from azure.core.credentials import AzureKeyCredential

import azure.functions as func

class DateTimeEncoder(JSONEncoder):
    #Override the default method    
    def default(self, obj):
        if isinstance(obj, (datetime.date, datetime.datetime)):
            return obj.isoformat()

def get_fields(result):
    fields = []
    for idx, doc in enumerate(result.documents):
        kvp = {}
        for kv_pair in doc.fields.items():
            if kv_pair[1].value_type == "string":
                kvp[kv_pair[0]] = kv_pair[1].content
            elif kv_pair[1].value_type == "list":
                line_items = []
                items = {}
                for rows in kv_pair[1].value:
                    for row in rows.value.items():
                        items[row[0]] = row[1].content
                        
                    line_items.append(items)

                kvp[kv_pair[0]] = line_items

        fields.append(kvp)
    return fields

def get_tables(result):
    tables = []
    for table_idx, table in enumerate(result.tables):
        cells = []
        for cell in table.cells: 
            cells.append( {
                "row_index": cell.row_index,
                "column_index": cell.column_index,
                "content": cell.content,
            })
        tab = {
                "row_count": table.row_count,
                "column_count": table.column_count,
                "cells": cells
        }
        tables.append(tab)
        return tables

def get_entities(result):
    entities = []
    for entity in result.entities:
        ent = {"content": format(entity.content),
        "category": format(entity.category),
        "sub_category": format(entity.sub_category),
        "confidence": format(entity.confidence)
        }
        entities.append(ent)
    return entities

def get_key_value_pairs(result):
    kvp = {}
    for kv_pair in result.key_value_pairs:
        if kv_pair.key:
            if kv_pair.value:
                kvp[kv_pair.key.content] = kv_pair.value.content
    return kvp

def get_pages(result):
    pages = []
    for page in result.pages:
        lines = []
        for line_idx, line in enumerate(page.lines):
            lines.append(line.content)
        pages.append(lines)
    return pages

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Invoked FormRecognizer Skill.')
    try:
        body = json.dumps(req.get_json())

        if body:
            logging.info(body)
            result = compose_response(body)
            logging.info("Result to return to custom skill: " + result)
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

def compose_response(json_data):
    values = json.loads(json_data)['values']
    
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    endpoint = os.environ["FORMS_RECOGNIZER_ENDPOINT"]
    key = os.environ["FORMS_RECOGNIZER_KEY"]
    
    for value in values:
        output_record = analyze_document(endpoint=endpoint, key=key, recordId=value["recordId"], data=value["data"])        
        results["values"].append(output_record)

    return json.dumps(results, ensure_ascii=False, cls=DateTimeEncoder)

def analyze_document(endpoint, key, recordId, data):
    try:
        formUrl = data["formUrl"] + data["formSasToken"]
        model = data["model"]
        logging.info("Model: " + model)
        document_analysis_client = DocumentAnalysisClient(
            endpoint=endpoint, credential=AzureKeyCredential(key)
        )
        poller = document_analysis_client.begin_analyze_document_from_url(
                model, formUrl)
        result = poller.result()
        logging.info("Result from Form Recognizer before formatting: " + str(result))
        output_record = {}
        output_record_data = {}
        if  model == "prebuilt-layout":
            output_record_data = { 
                "tables": get_tables(result),
                "pages": get_pages(result)
        }
        elif model == "prebuilt-document":
            output_record_data = { 
                "kvp": get_key_value_pairs(result),
                "entities" : get_entities(result),
                "tables": get_tables(result),
                "pages": get_pages(result)
            }
        elif model == "prebuilt-receipt":
            output_record_data = { 
                "fields": get_fields(result),
                "tables": get_tables(result),
                "pages": get_pages(result)
            }
        elif model == "prebuilt-idDocument":
            output_record_data = { 
                "fields": get_fields(result),
                "tables": get_tables(result),
                "pages": get_pages(result)
            }
        elif model == "prebuilt-invoice":
            output_record_data = { 
                "fields": get_fields(result),
                "tables": get_tables(result),
                "pages": get_pages(result)
        }
        else:
            output_record_data = { 
                "kvp": get_fields(result),
                "tables": get_tables(result),
                "pages": get_pages(result)
            }

        output_record = {
            "recordId": recordId,
            "data": output_record_data
        }

    except Exception as error:
        output_record = {
            "recordId": recordId,
            "errors": [ { "message": "Error: " + str(error) }   ] 
        }

    logging.info("Output record: " + json.dumps(output_record, ensure_ascii=False, cls=DateTimeEncoder))
    return output_record

        
