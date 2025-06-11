
from azure.core.credentials import AzureKeyCredential
from azure.search.documents.models import VectorizedQuery
from azure.search.documents import SearchClient
from promptflow.connections import CognitiveSearchConnection
from promptflow.core import tool


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def search_azure_ai_search(vector: list,aisearchconn: CognitiveSearchConnection) -> str:

    search_client = SearchClient(endpoint=aisearchconn.api_base, index_name='slmindex', credential=AzureKeyCredential(aisearchconn.api_key))

    vector_query = VectorizedQuery(vector=vector, k_nearest_neighbors=3, fields="chunking_vector")
  
    results = search_client.search(  
        search_text=None,  
        vector_queries= [vector_query],
        select=["chunking"],
    ) 


    content = ''
  
    for result in results:  
        content += f"{result['chunking']}"
        print(f"Chunking: {result['chunking']}")  
        print(f"Score: {result['@search.score']}")  

    return content
