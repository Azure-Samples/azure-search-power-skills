import os
import pypdf
import json
from promptflow.core import tool


# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def extract_pdf_table(file_path: str):
    pdf_reader = pypdf.PdfReader(file_path)
    # n_pages = len(pdf_reader.pages)
    output_directory = './files/imgs'
    if not os.path.exists(output_directory):
        os.mkdir(output_directory)

    imgs = []
    page_num = []

    print('Extracting images from pdf file...')

    for page in pdf_reader.pages:
        for image in page.images:
            page_num = page.page_number+1
            with open(os.path.join(output_directory,image.name), "wb") as fp:
                fp.write(image.data)
                page_info = {
                    "page_number": page_num,
                    "image_path": os.path.join(output_directory, image.name)
                }
                # print(page_info)
                imgs.append(page_info)


    
    return imgs
