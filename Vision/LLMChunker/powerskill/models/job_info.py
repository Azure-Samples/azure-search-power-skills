import uuid
from models.app_config import AppConfig
from models.skill_output import ResponseChunk

class JobInfo:
    def __init__(self, 
                 id: str = '', 
                 root_folder: str = None, 
                 original_doc_path: str = '', 
                 original_doc_extension: str = '', 
                 original_doc_name_without_extension: str = '', 
                 pdf_doc_path: str = '', 
                 images_folder: str = '', 
                 images_files: list[str] = None, 
                 markdown_folder: str = '', 
                 markdown_files: list[str] = None, 
                 markdown_merged: str = '', 
                 responseChunks: list[ResponseChunk] = None, 
                 appConfig: AppConfig = None):
        self.id = str(uuid.uuid4())
        self.root_folder = root_folder
        self.original_doc_path = original_doc_path
        self.original_doc_extension = original_doc_extension
        self.original_doc_name_without_extension = original_doc_name_without_extension
        self.pdf_doc_path = pdf_doc_path
        self.images_folder = images_folder
        self.images_files = images_files if images_files is not None else []
        self.markdown_folder = markdown_folder
        self.markdown_files = markdown_files if markdown_files is not None else []
        self.markdown_merged = markdown_merged
        self.responseChunks = responseChunks if responseChunks is not None else []
        self.appConfig = appConfig