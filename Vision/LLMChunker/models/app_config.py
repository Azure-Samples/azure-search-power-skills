import os

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
            self.extraction_prompt = """Extract everything you see in these images to raw markdown. Do NOT include the markdown code block in the beginning of your response. Respect the following rules for each visual element:
            - Charts such as line, pie, bar charts and tables: convert to markdown tables.
            - Diagrams and flowcharts: describe in text in details."""

    def ReadVariable(self, env_var_name: str, required: bool) -> str:
        value = os.getenv(env_var_name)
        if required and value is None:
            raise EnvironmentError(f"Required environment variable '{env_var_name}' not found.")
        return value.strip() if value is not None else None