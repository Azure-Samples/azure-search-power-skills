import time
from embedder.text_embedder import TextEmbedder

class ChunkEmbeddingHelper():
    def __init__(self):
        self.text_embedder = TextEmbedder()

    def generate_chunks_with_embedding(self, document_id, content_chunks, fieldname, sleep_interval_seconds) ->  dict:
        offset = 0
        chunk_embeddings = []
        for index, (content_chunk) in enumerate(content_chunks):
            metadata = self._generate_content_metadata(document_id, fieldname, index, content_chunk, offset)
            offset += metadata['length']
            chunk_embeddings.append(metadata)
            
            # A very crude way to introduce some delay between each embedding call
            # This is to avoid hitting the rate limit of the OpenAI API
            time.sleep(sleep_interval_seconds)
        return chunk_embeddings

    def _generate_content_metadata(self, document_id, fieldname, index, content, offset):
        metadata = {'fieldname':fieldname}
        metadata['docid'] = document_id
        metadata['index'] = index
        metadata['offset'] = offset
        metadata['length'] = len(content)
        metadata['embedding'] = self.text_embedder.embed_content(content)
        return metadata