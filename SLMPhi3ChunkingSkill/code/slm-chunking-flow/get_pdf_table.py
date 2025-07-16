import base64
import json
from azure.core.credentials import AzureKeyCredential
from azure.ai.documentintelligence import DocumentIntelligenceClient
from azure.ai.documentintelligence.models import AnalyzeResult
from azure.ai.documentintelligence.models import AnalyzeDocumentRequest

from promptflow.core import tool

endpoint = "https://kinfey-azure-ai-service.cognitiveservices.azure.com/"
key = "1b18187e70394496b5a0aef5e65d07cf"

# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def extract_pdf_table(file_path: str) -> list:

    document_intelligence_client = DocumentIntelligenceClient(
        endpoint=endpoint, credential=AzureKeyCredential(key)
    )
    with open(file_path, "rb") as f:
        base64_encoded_pdf = base64.b64encode(f.read()).decode("utf-8")

    analyze_request = {
        "base64Source": base64_encoded_pdf
    }

    poller = document_intelligence_client.begin_analyze_document(
        "prebuilt-layout", analyze_request=analyze_request
    )

    result = poller.result()

    page = 0
    md_content = ""

    tables = []


    print('Extracting tables from pdf file...')

    if result.tables:
        for table_idx, table in enumerate(result.tables):
            if table.bounding_regions:
                for region in table.bounding_regions:
                    page = region.page_number
            md_content = convert_table_to_markdown(table)
            json_output = json.dumps({"page_number": page, "md_content": md_content}, ensure_ascii=False)



            # print(json_output)
            tables.append(json_output)


    
    return tables

def convert_table_to_markdown(table):
    max_row = max(cell.row_index for cell in table.cells) + 1
    max_col = max(cell.column_index for cell in table.cells) + 1

    markdown_table = [["" for _ in range(max_col)] for _ in range(max_row)]

    for cell in table.cells:
        markdown_table[cell.row_index][cell.column_index] = cell.content

    markdown_str = ""
    for row in markdown_table:
        markdown_str += "| " + " | ".join(row) + " |\n"
        markdown_str += "| " + " | ".join(["---"] * len(row)) + " |\n"
        break  
    for row in markdown_table[1:]:
        markdown_str += "| " + " | ".join(row) + " |\n"

    return markdown_str
