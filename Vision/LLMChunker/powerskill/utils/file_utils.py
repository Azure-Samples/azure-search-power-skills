import base64
import json
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
        identity_id = await FileUtils.get_identity_id() 
        async with DefaultAzureCredential(logging_enable=True) as default_credential:
            try:
                async with BlobServiceClient(account_url, credential=default_credential) as blob_service_client:
                    container_blob_name = FileUtils.extract_container_and_blob_name(azureBlobUrl)
                    container_name = container_blob_name[0]
                    blob_name = container_blob_name[1]

                    blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)

                    with open(file=file_path, mode="wb") as blob:
                        download_stream = await blob_client.download_blob(validate_content=True)
                        data = await download_stream.readall()
                        blob.write(data)
                    
                    await blob_client.close()
                    await blob_service_client.close()
            except Exception as e:
                if e.status_code == 403:
                    raise Exception(f"Permission error accessing the storage account. Please assign at least 'Storage Blob Data Reader' role to the identity {identity_id} in the storage account {account_url}") from e

        end_time = time.time() 
        elapsed_time = (end_time - start_time) * 1000  # Calculate elapsed time in milliseconds
        print(f"Elapsed time to download: {elapsed_time:.2f}ms")

    @staticmethod
    async def get_identity_id() -> str:
        async with DefaultAzureCredential(logging_enable=True) as default_credential:
            try:
                token = await default_credential.get_token("https://graph.microsoft.com/.default")
                base64_meta_data = token.token.split(".")[1].encode("utf-8") + b'=='
                json_bytes = base64.decodebytes(base64_meta_data)
                json_string = json_bytes.decode("utf-8")
                json_dict = json.loads(json_string)
                current_identity = json_dict["oid"]
                return current_identity
            except:
                raise Exception("No identity found using DefaultAzureCredential(). If you are running this application locally, please make sure you are logged using the Azure CLI. Or, if you are running this app in Azure, make sure to enable Managed Identity for the service.")
