import os  
import json  
from openai import AzureOpenAI


from promptflow.core import tool
from promptflow.connections import AzureOpenAIConnection


class EmbeddingTrunking:

    aoai =None

    @staticmethod
    def init_aoai(aoai_conn: AzureOpenAIConnection):
        if EmbeddingTrunking.aoai is None:
            EmbeddingTrunking.aoai = AzureOpenAI(api_key = aoai_conn.api_key, api_version = aoai_conn.api_version, azure_endpoint = aoai_conn.api_base)

    @staticmethod
    def generate_embeddings(text: str, aoai_conn: AzureOpenAIConnection):
        EmbeddingTrunking.init_aoai(aoai_conn)
        response = EmbeddingTrunking.aoai.embeddings.create(
            input=text, model="EmbeddingModel")
        embeddings = response.data[0].embedding
        return embeddings

@tool
def embedding_with_aoai(contents: list,aoai_conn: AzureOpenAIConnection) -> list: 

    i = 1000
    content_list = contents
    for item in content_list:
        content = item['chunking']
        content_embeddings = EmbeddingTrunking.generate_embeddings(content,aoai_conn)
        item["id"] = str(i)
        item['chunking_vector'] = content_embeddings
        i += 1
    return content_list
