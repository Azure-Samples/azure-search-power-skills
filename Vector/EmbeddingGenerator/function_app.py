import azure.functions as func
import logging
import json
from chunker.text_chunker import TextChunker
from chunker.chunk_metadata_helper import ChunkEmbeddingHelper

app = func.FunctionApp()

TEXT_CHUNKER = TextChunker()
CHUNK_METADATA_HELPER = ChunkEmbeddingHelper()

"""
Required environment variables:
"AZURE_OPENAI_API_KEY"
"AZURE_OPENAI_API_VERSION"
"AZURE_OPENAI_EMBEDDING_DEPLOYMENT"
"AZURE_OPENAI_SERVICE_NAME"
"""

@app.function_name(name="TextEmbedder")
@app.route(route="chunk-embed")
def text_chunking(req: func.HttpRequest) -> func.HttpResponse:

    logging.info('Python HTTP trigger function processed a request.')

    request = req.get_json()
    
    recordId = request['values'][0]['recordId']
    document_id = request['values'][0]['data']['document_id']
    text = request['values'][0]['data']['text']
    filepath = request['values'][0]['data']['filepath']
    fieldname = request['values'][0]['data']['fieldname']

    # chunk documents into chunks of (by default) 1024 tokens, and for each chunk, generate the vector embedding
    chunking_result = TEXT_CHUNKER.chunk_content(text, file_path=filepath)
    content_chunk_metadata = CHUNK_METADATA_HELPER.generate_chunks_with_embedding(document_id, [c.content for c in chunking_result.chunks], fieldname)

    for document_chunk, embedding_metadata in zip(chunking_result.chunks, content_chunk_metadata):
        document_chunk.embedding_metadata = embedding_metadata

    response_body = {
        "values": [
            {
                "recordId": recordId,
                "data": chunking_result,
                "errors": None,
                "warnings": None            
            }
        ]
    }

    logging.info(f'Python HTTP trigger function created {len(chunking_result.chunks)} chunks.')

    response = func.HttpResponse(json.dumps(response_body, default=lambda obj: obj.__dict__))
    response.headers['Content-Type'] = 'application/json'    
    return response