from pydantic import BaseModel
from typing import List

class ResponseChunk(BaseModel):
    chunk_id: str
    file_name: str
    content: str
    title: str

class ResponseData(BaseModel):
    chunks: List[ResponseChunk]
    markdown: str

class ResponseValue(BaseModel):
    data: ResponseData | None = None
    recordId: str
    errors: List[str] | None = None
    warnings: List[str] | None = None

class ResponseProcess(BaseModel):
    values: List[ResponseValue]