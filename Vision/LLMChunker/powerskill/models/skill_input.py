from pydantic import BaseModel
from typing import List

class RequestData(BaseModel):
    blobUrl: str
    chunk_size: int | None = None
    chunk_overlap: int | None = None
    extraction_prompt: str | None = None
    image_quality: str | None = None

class RequestValue(BaseModel):
    data: RequestData
    recordId: str

class RequestProcess(BaseModel):
    values: List[RequestValue]