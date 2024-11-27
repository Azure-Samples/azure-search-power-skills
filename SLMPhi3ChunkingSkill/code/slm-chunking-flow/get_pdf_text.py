
import pypdf
import json

from promptflow.core import tool


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def extract_pdf_text(file_path: str) -> str:
    pdf_reader = pypdf.PdfReader(file_path)
    n_pages = len(pdf_reader.pages)
    pages_text = []


    print('Extracting text from pdf file...')

    for i in range(n_pages):
        pages_text.append({'page_number': i + 1, 'text': pdf_reader.pages[i].extract_text()})

    content = ''

    for item in pages_text:
        content += item['text']
        # print(item['text'])
    return content