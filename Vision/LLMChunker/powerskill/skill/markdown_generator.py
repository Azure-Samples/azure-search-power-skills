import asyncio
import os
from pathlib import Path
import time
import fitz
import shutil
import re
import subprocess
from langchain.text_splitter import MarkdownHeaderTextSplitter
from langchain_text_splitters import RecursiveCharacterTextSplitter
from utils.image_utils import ImageUtils
from models.app_config import AppConfig
from utils.file_utils import FileUtils
from models.job_info import JobInfo
from models.skill_input import RequestData
from models.skill_output import ResponseChunk, ResponseData
from openai import AsyncAzureOpenAI, OpenAIError 
from azure.identity.aio import DefaultAzureCredential, get_bearer_token_provider
               
async def process(input: RequestData) -> ResponseData:  
    try:
        job = JobInfo()
        job.appConfig = AppConfig()
        job.appConfig.LoadFromRequest(input)

        setup_directories(job)
        await download_file(job, input)
        convert_to_pdf(job)
        extract_pdf_pages_to_images(job)
        await convert_images_to_markdown(job)
        merge_markdown_files(job)
        chunk_markdown(job)

        return ResponseData(chunks=job.responseChunks, markdown=job.markdown_merged)
    except Exception as ex:
        print(f"An unknown occurred while processing: {ex}")
        raise ex
    finally:
        if (job.root_folder is not None):
            remove_directory(job.root_folder)

def create_directory(directory_path) -> str:  
    path = Path(directory_path)  
    if not path.exists():  
        path.mkdir(parents=True, exist_ok=True)
    return str(path.absolute())

def remove_directory(directory_path):  
    try:  
        if os.path.exists(directory_path):  
            shutil.rmtree(directory_path)  
            print(f"Directory '{directory_path}' has been removed successfully.")  
        else:  
            print(f"Directory '{directory_path}' does not exist.")  
    except Exception as e:  
        print(f"An error occurred while removing the directory: {e}")  

def setup_directories(job: JobInfo): 
    job.root_folder = create_directory(job.id) 
    job.original_doc_path = create_directory(os.path.join(job.root_folder, 'original_doc'))
    job.pdf_doc_path = create_directory(os.path.join(job.root_folder, 'pdf_doc'))
    job.images_folder = create_directory(os.path.join(job.root_folder, 'images'))
    job.markdown_folder = create_directory(os.path.join(job.root_folder, 'markdown'))


async def download_file(job: JobInfo, input: RequestData):  
    job.original_doc_extension = FileUtils.extract_extension(input.blobUrl)
    job.original_doc_name_without_extension = FileUtils.extract_filename_without_extension(input.blobUrl)
    job.original_doc_path = os.path.join(job.original_doc_path, f"{job.original_doc_name_without_extension}.{job.original_doc_extension}")

    print(f"Downloading file from {input.blobUrl} to {job.original_doc_path}")

    await FileUtils.download_from_azure_blob(input.blobUrl, job.original_doc_path)

def convert_to_pdf(job: JobInfo):
    if (job.original_doc_extension == 'pdf'):
        job.pdf_doc_path = job.original_doc_path
        print(f"Original document is already a PDF, no conversion needed")
    elif (job.original_doc_extension in ['pptx', 'ppt', 'docx', 'doc']):  
        print(f"Converting file extension {job.original_doc_extension} to pdf...")
        start_time = time.time()
        command = [  
            'soffice',  # or 'libreoffice' depending on your installation  
            '--headless', 
            '--convert-to', 'pdf',
            '--outdir', os.path.dirname(job.pdf_doc_path),
            job.original_doc_path
        ]  
        subprocess.run(command, check=True)  
        job.pdf_doc_path = os.path.join(os.path.dirname(job.pdf_doc_path), f"{job.original_doc_name_without_extension}.pdf")
        end_time = time.time() 
        elapsed_time = (end_time - start_time) * 1000
        
        print(f"Conversion completed. Elapsed time: {elapsed_time:.2f}ms")

def extract_pdf_pages_to_images(job: JobInfo):
    start_time = time.time()

    if job.pdf_doc_path is None:  # The original document is already an image
        print('Converting image to the right resolution...')
        new_image = ImageUtils.resize_image(job.original_doc_path, job.appConfig.image_quality)
        job.images_files.append(new_image)
    else:
        print('Extracting images from PDF...')

        pdf_document = fitz.open(job.pdf_doc_path)
        pg_counter = 0
        for page_number in range(len(pdf_document)):
            page = pdf_document.load_page(page_number)         
            dpi = ImageUtils.calculate_dpi_from_page(page, job.appConfig.image_quality)
            image = page.get_pixmap(dpi=dpi)
            image_out_file = os.path.join(job.images_folder, f'{(page_number + 1):09}.png')
            image.save(image_out_file)
            print(f"Extracted page {page_number + 1} to {image_out_file}")
            job.images_files.append(image_out_file)
            pg_counter += 1
    
    end_time = time.time()
    elapsed_time = (end_time - start_time) * 1000
    print(f"Images conversion completed. Elapsed time: {elapsed_time:.2f}ms")

async def process_image(job: JobInfo, files: list, batch_number: int):  
    print('Calling OpenAI for images...:')

    for f in files:  
        print(f)

    start_time = time.time()
    markdown_text = await extract_markdown_from_images(job=job, image_paths=files)  
    
    markdown_file_out = os.path.join(job.markdown_folder, f'{batch_number:09}.txt')  
    with open(markdown_file_out, 'w') as md_out:  
        md_out.write(markdown_text)
        job.markdown_files.append(markdown_file_out)
    
    end_time = time.time() 
    elapsed_time = (end_time - start_time) * 1000
    print(f'Received response in {elapsed_time:.2f} ms. Writing markdown to file.')

async def extract_markdown_from_images(job: JobInfo, image_paths: list) -> list:  
    counter = 0  
    incremental_backoff = 1 

    if job.appConfig.image_quality == "low":
        detail = "low"
    else:
        detail = "high"

    async with DefaultAzureCredential() as credential:
        token_provider = get_bearer_token_provider(credential, "https://cognitiveservices.azure.com/.default")
        
        try:  
            base64_images = [ImageUtils.encode_image(image_path) for image_path in image_paths]
            gpt_client = AsyncAzureOpenAI(   
                api_version=job.appConfig.openai_api_version,  
                azure_endpoint=job.appConfig.openai_url,
                azure_deployment=job.appConfig.openai_deployment,
                azure_ad_token_provider=token_provider
            )  

            firstPageNumber = FileUtils.extract_filename_without_extension(image_paths[0]).lstrip('0')
            lastPageNumber = FileUtils.extract_filename_without_extension(image_paths[-1]).lstrip('0')

            while True and counter < job.appConfig.openai_max_retries:  
                messages = [
                    {"role": "system", "content": job.appConfig.extraction_prompt},
                    {"role": "user", "content": f"Document type: {job.original_doc_extension}. Page numbers: {firstPageNumber} to {lastPageNumber}"},
                ]
                for base64_image in base64_images:
                    messages.append({"role": "user", "content": [{"type": "image_url", "image_url": {"url": f"data:image/png;base64,{base64_image}", "detail": f"{detail}"}}]})

                response = await gpt_client.chat.completions.create(  
                    model=job.appConfig.openai_deployment,  
                    messages=messages,
                    max_tokens=int(job.appConfig.openai_max_tokens_response * len(image_paths))
                )  
                
                return response.choices[0].message.content
        except OpenAIError as ex:  
            if str(ex.code) == "429":  
                print('OpenAI Throttling Error - Waiting to retry after', incremental_backoff, 'seconds...')  
                incremental_backoff = min(job.appConfig.openai_max_backoff, incremental_backoff * 1.5)  
                counter += 1  
                await asyncio.sleep(incremental_backoff)  
            elif str(ex.code) == "content_filter":  
                print('Content Filter Error', ex.code)  
                return ["Content could not be extracted due to Azure OpenAI content filter." + ex.code] * len(image_paths)
            elif str(ex.code) == "PermissionDenied":  
                raise Exception(f"Permission error accessing OpenAI. Please assign 'Cognitive Services OpenAI User' role to the identity {await FileUtils.get_identity_id()} in the OpenAI account {job.appConfig.openai_url}")
            else:  
                raise ex
        except Exception as ex:  
            raise ex
        
async def convert_images_to_markdown(job: JobInfo) -> str:
    semaphore = asyncio.Semaphore(job.appConfig.openai_max_concurrent_requests)

    async def sem_process_image(job, image_files, i):
        async with semaphore:
            await process_image(job, image_files, i)

    # Group images into batches. The more we can fit within the same request, the better. It keeps the context of the images together as a single document.
    image_batches = [job.images_files[i:i + job.appConfig.openai_max_images_per_request] for i in range(0, len(job.images_files), job.appConfig.openai_max_images_per_request)]
    number_batches = len(image_batches)

    if number_batches > job.appConfig.openai_max_concurrent_requests:
        print('The number of concurrent requests to OpenAI will be limited to ' + str(job.appConfig.openai_max_concurrent_requests))

    tasks = []

    for i in range(number_batches):
        tasks.append(sem_process_image(job, image_batches[i], i))

    await asyncio.gather(*tasks)

def merge_markdown_files(job: JobInfo) -> str:  
    sorted_files = sorted(job.markdown_files)  
    merged_markdown = ''  
    for f in sorted_files:  
        with open(f, 'r') as f_in:  
            pg_number = int(Path(f).stem)
            if pg_number > 1:
                merged_markdown += f'\n||{int(pg_number)}||\n' + f_in.read() + '\n'  
            else:
                merged_markdown += f_in.read() + '\n'

    markdown_file = os.path.join(job.markdown_folder, 'merged_markdown.txt')
    print('Writing merged markdown file to ' + markdown_file)  
    with open(markdown_file, 'w') as f_out:  
        f_out.write(merged_markdown)  
    job.markdown_merged = merged_markdown

def chunk_markdown(job: JobInfo):  
    headers_to_split_on = [
        ("#", "Header 1"),
        ("##", "Header 2"),
        ("###", "Header 3")
    ]

    markdown_splitter = MarkdownHeaderTextSplitter(
        headers_to_split_on=headers_to_split_on, strip_headers=False
    )

    text_splitter = RecursiveCharacterTextSplitter.from_tiktoken_encoder(
        model_name="gpt-4",
        chunk_size=job.appConfig.chunk_size,
        chunk_overlap = (job.appConfig.chunk_size * job.appConfig.chunk_overlap) // 100
    )

    md_header_splits = markdown_splitter.split_text(job.markdown_merged)
    # Token-level splits
    splits = text_splitter.split_documents(md_header_splits)
    chunk_id = 1  
    pg_number = 1  
    
    last_heading = ''
    
    for s in splits:
        # Add the page number to start of chunk 
        text = s.page_content

        find_heading = find_last_heading(text)  
        if find_heading != None:
            last_heading = find_heading
        
        get_pg_number  = find_first_page_number(text) 
        if text.find('||') != 0:
            if get_pg_number != None:
                pg_number = get_pg_number
        #text = '||' + str(pg_number-1) + '||\n' + last_heading + '\n' + text
        
        job.responseChunks.append(ResponseChunk(
            chunk_id=f"{job.id}-{chunk_id}",
            file_name=os.path.basename(job.original_doc_path),
            content=text,
            title=last_heading.replace('#', '').strip()
        ))

        chunk_id += 1    

def find_last_heading(markdown_text):  
    lines = markdown_text.split('\n')  
    last_heading = None  
      
    for line in lines:
        if line.startswith('### '):    
            last_heading = line
        elif line.startswith('## '):  
            last_heading = line
        elif line.startswith('# '):  
            last_heading = line  
      
    return last_heading  

def find_first_page_number(text):  
    pattern = r'\|\|(\d+)\|\|'  
    matches = re.findall(pattern, text)  
    if matches:  
        return int(matches[0])  
    else:  
        return None 