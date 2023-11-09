import azure.functions as func
import logging
import json
import jsonschema
from embedder.text_embedder import TextEmbedder

app = func.FunctionApp()
EMBEDDING_HELPER = TextEmbedder()

@app.function_name(name="TextEmbedder")
@app.route(route="embed")
def text_chunking(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Python HTTP trigger function processed a request.')
    request = req.get_json()

    try:
        jsonschema.validate(request, schema=get_request_schema())
    except jsonschema.exceptions.ValidationError as e:
        return func.HttpResponse("Invalid request: {0}".format(e), status_code=400)

    texts = []
    for value in request["values"]:
        texts.append(value["data"]["text"])

    embeddings = EMBEDDING_HELPER.generate_embeddings(texts)

    values = []
    for index, value in enumerate(request['values']):
        recordId = value['recordId']
        embedding = embeddings[index]
        values.append({
            "recordId": recordId,
            "data": {"embedding": embedding.tolist()},
            "errors": None,
            "warnings": None
        })

    response_body = { "values": values }

    logging.info(f'Python HTTP trigger function created {len(values)} embeddings.')

    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'    
    return response

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
                                "text": {"type": "string", "minLength": 1}
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