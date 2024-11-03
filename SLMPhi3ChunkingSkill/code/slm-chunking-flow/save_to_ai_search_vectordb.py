
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import (
    SearchField,
    SearchIndex,
    SearchFieldDataType,
    VectorSearch,
    HnswAlgorithmConfiguration,
    VectorSearchAlgorithmMetric,
    ExhaustiveKnnAlgorithmConfiguration,
    ExhaustiveKnnParameters,
    VectorSearchProfile,
    AzureOpenAIVectorizer,
    AzureOpenAIParameters,
    SemanticConfiguration,
    SemanticSearch,
    SemanticPrioritizedFields,
    SemanticField
)

from promptflow.core import tool
from promptflow.connections import AzureOpenAIConnection,CognitiveSearchConnection


def initSearch(service_endpoint : str ,index_name: str,credential: AzureKeyCredential,aoai_conn: AzureOpenAIConnection):
  # Create a search index
  index_client = SearchIndexClient(
      endpoint=service_endpoint, credential=credential)
  


  fields = [
      SearchField(name="id",type=SearchFieldDataType.String, key=True, sortable=True, filterable=True, facetable=True),
      SearchField(name="chunking", type=SearchFieldDataType.String),
      SearchField(name="chunking_vector", type=SearchFieldDataType.Collection(SearchFieldDataType.Single),searchable=True, vector_search_dimensions=1536, vector_search_profile_name="myHnswProfile")
  ]

  vector_search = VectorSearch(
    algorithms=[
        HnswAlgorithmConfiguration(
            name="myHnsw"
        )
    ],
    profiles=[
        VectorSearchProfile(
            name="myHnswProfile",
            algorithm_configuration_name="myHnsw",
            vectorizer="myVectorizer"
        )
    ],
    vectorizers=[
        AzureOpenAIVectorizer(
            name="myVectorizer",
            azure_open_ai_parameters=AzureOpenAIParameters(
                resource_uri=aoai_conn.api_base,
                deployment_id="EmbeddingModel",  
                api_key=aoai_conn.api_key,  
                model_name="text-embedding-ada-002"
            )
        )
    ]
  )

#   vector_search = VectorSearch(
#       algorithm=[
#           HnswAlgorithmConfiguration(
#               name="slm-semantic-config",
#               kind="hnsw",
#               hnsw_parameters={
#                   "m": 4,
#                   "efConstruction": 400,
#                   "efSearch": 500,
#                   "metric": "cosine"
#               }
#           )
#       ], 
#       profiles=[
#         VectorSearchProfile(
#             name="luk",
#             algorithm_configuration_name="slm-semantic-config",
#             vectorizer="myVectorizer"
#         )
#       ], 
#       vectorizers=[  
#         AzureOpenAIVectorizer(  
#             name="myOpenAI",  
#             kind="azureOpenAI",  
#             azure_open_ai_parameters=AzureOpenAIParameters(  
#                 resource_uri=aoai_conn.api_base,  
#                 deployment_id="EmbeddingModel",  
#                 api_key=aoai_conn.api_key,  
#             ),  
#         ),  
#       ],
#   )

#   semantic_config = SemanticConfiguration(
#       name="my-semantic-config",
#       prioritized_fields=PrioritizedFields(
#           title_field=SemanticField(field_name="KB"),
#           prioritized_content_fields=[SemanticField(field_name="Content")]
#       )
#   )


  semantic_config = SemanticConfiguration(  
        name="my-semantic-config",  
        prioritized_fields=SemanticPrioritizedFields(  
            content_fields=[SemanticField(field_name="chunking")]
            # vector_fields=[SemanticField(field_name="chunking_vector")]
        ),  
  )  
  
  # Create the semantic search with the configuration  
  semantic_search = SemanticSearch(configurations=[semantic_config])  
    
   # Create the search index
  index = SearchIndex(name=index_name, fields=fields, vector_search=vector_search, semantic_search=semantic_search)  
  result = index_client.create_or_update_index(index)  

  return result

#   semantic_settings = SemanticSettings(configurations=[semantic_config])

#   index = SearchIndex(name=index_name, fields=fields,
#                       vector_search=vector_search, semantic_settings=semantic_settings)
#   result = index_client.create_or_update_index(index)


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def save_data_embeddings(vector_json: list, cogconn: CognitiveSearchConnection, aoaiconn: AzureOpenAIConnection) -> str:
    documents = vector_json 
    service_endpoint = cogconn.api_base
    index_name = 'slmindex'
    key = cogconn.api_key
    credential = AzureKeyCredential(key)
    initSearch(service_endpoint,index_name,credential,aoaiconn)

    search_client = SearchClient(endpoint=service_endpoint, index_name=index_name, credential=credential)
    result = search_client.upload_documents(documents) 
    return 'done'
