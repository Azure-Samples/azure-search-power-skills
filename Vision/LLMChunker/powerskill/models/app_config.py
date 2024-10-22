import os

from models.skill_input import RequestData

class AppConfig():
    openai_url: str
    openai_deployment: str
    openai_api_version: str
    openai_max_concurrent_requests: int
    openai_max_images_per_request: int
    openai_max_retries: int
    openai_max_backoff: int
    openai_max_tokens_response: int
    extraction_prompt: str
    chunk_size: int
    chunk_overlap: int
    image_quality: str

    def __init__(self):
        self.LoadEnvironmentVariables()

    def LoadEnvironmentVariables(self):
        self.openai_url = self.ReadVariable('OPENAI_URL', required=True)
        self.openai_deployment = self.ReadVariable('OPENAI_DEPLOYMENT', required=True)
        self.openai_api_version = self.ReadVariable('OPENAI_API_VERSION', required=True)
        self.openai_max_concurrent_requests = int(self.ReadVariable('OPENAI_MAX_CONCURRENT_REQUESTS', required=True))
        self.openai_max_images_per_request = int(self.ReadVariable('OPENAI_MAX_IMAGES_PER_REQUEST', required=True))
        self.openai_max_retries = int(self.ReadVariable('OPENAI_MAX_RETRIES', required=True))
        self.openai_max_backoff = int(self.ReadVariable('OPENAI_MAX_BACKOFF', required=True))
        self.openai_max_tokens_response = int(self.ReadVariable('OPENAI_MAX_TOKENS_RESPONSE', required=True))
        self.chunk_size = int(self.ReadVariable('CHUNK_SIZE', required=True))
        self.chunk_overlap = int(self.ReadVariable('CHUNK_OVERLAP', required=True))
        self.extraction_prompt = self.ReadVariable('EXTRACTION_PROMPT', required=False)
        self.image_quality = self.ReadVariable('IMAGE_QUALITY', required=True)

        if self.extraction_prompt is None:
            self.extraction_prompt = """Extract everything you see in these images to raw markdown. All images belong to a single document (its pages). Do NOT include the markdown code block in the beginning of your response. Do NOT summarize. Respect the following rules for each visual element:
            - Charts such as line, pie, bar charts: convert to markdown tables keeping all data. If converting to table is not possible, describe the chart in details using textual or table presentation, and business analysis and insights.
            - Diagrams, flow charts: describe all process in details, do NOT simplify. Use bullet lists with identation to describe the process.
            - Tables: keep all data, do NOT summarize or change the original information, even if the table is large. If the same table breaks into multiple images, keep the data in the same table in the markdown output.
            
            The images you are receiving are pages of a document. You will be given what is the document extension (e.g. pdf, docx, pptx) and what page numbers (or slides in case of ppt) you are processing. For each document extension listed below, respect the following rules:
            - PDF: For page 1, always extract the document title with markdown heading 1 (#). For subsequent pages, make your best judgement to convert the headings to appropriate markdown headings.
            - DOCX and DOC: For page 1, always extract the document title with markdown heading 1 (#). For subsequent pages, make your best judgement to convert the headings to appropriate markdown headings.
            - PPTX and PPT: For slide 1, always extract the document title with markdown heading 1 (#). For subsequent slides, extract their titles as heading 2 (##). If inside each slide there are more heading, make them heading 3 (###) .
            """

    def ReadVariable(self, env_var_name: str, required: bool) -> str:
        value = os.getenv(env_var_name)
        if required and value is None:
            raise EnvironmentError(f"Required environment variable '{env_var_name}' not found.")
        return value.strip() if value is not None else None
    
    def LoadFromRequest(self, request_data: RequestData):
        if request_data.chunk_size is not None:
            self.chunk_size = request_data.chunk_size
        if request_data.chunk_overlap is not None:
            self.chunk_overlap = request_data.chunk_overlap
        if request_data.extraction_prompt is not None:
            self.extraction_prompt = request_data.extraction_prompt
        if request_data.image_quality is not None:
            self.image_quality = request_data.image_quality