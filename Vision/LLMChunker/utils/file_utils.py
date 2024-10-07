import os
import time
from urllib.parse import urlparse
from typing import Tuple
from azure.identity.aio import DefaultAzureCredential
from azure.storage.blob.aio import BlobServiceClient

class FileUtils:
    def extract_container_and_blob_name(url: str) -> Tuple[str, str]:
        parsed_url = urlparse(url)
        path_parts = parsed_url.path.lstrip('/').split('/', 1)
        container_name = path_parts[0]
        blob_name = path_parts[1] if len(path_parts) > 1 else ''
        return container_name, blob_name

    @staticmethod
    def extract_root_domain(url: str) -> str:
        parsed_url = urlparse(url)
        root_domain = f"{parsed_url.scheme}://{parsed_url.netloc}"
        return root_domain
    
    @staticmethod
    def extract_file_name(url: str) -> str:
        parsed_url = urlparse(url)
        path_parts = parsed_url.path.rstrip('/').split('/')
        return path_parts[-1] if path_parts else ''
    
    @staticmethod
    def extract_filename_without_extension(url: str) -> str:
        file_name = FileUtils.extract_file_name(url)
        return os.path.splitext(file_name)[0]
    
    @staticmethod
    def extract_extension(url: str) -> str:
        file_name = FileUtils.extract_file_name(url)
        return os.path.splitext(file_name)[1].lstrip('.')

    @staticmethod
    async def download_from_azure_blob(azureBlobUrl: str, file_path: str):
        start_time = time.time()  # Record the start time

        account_url = FileUtils.extract_root_domain(azureBlobUrl)
        async with DefaultAzureCredential() as default_credential:
            async with BlobServiceClient(account_url, credential=default_credential) as blob_service_client:
                container_blob_name = FileUtils.extract_container_and_blob_name(azureBlobUrl)
                container_name = container_blob_name[0]
                blob_name = container_blob_name[1]

                blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)

                with open(file=file_path, mode="wb") as blob:
                    download_stream = await blob_client.download_blob(validate_content=True)
                    data = await download_stream.readall()
                    blob.write(data)

        end_time = time.time() 
        elapsed_time = (end_time - start_time) * 1000  # Calculate elapsed time in milliseconds
        print(f"Elapsed time to download: {elapsed_time:.2f}ms")
