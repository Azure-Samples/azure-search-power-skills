"Data model for prompts"
from dataclasses import dataclass
from typing import Dict, Optional


@dataclass
class Document():
    """A data class for storing documents

    Attributes:
        content (str): The content of the document.
        id (Optional[str]): The id of the document.
        title (Optional[str]): The title of the document.
        filepath (Optional[str]): The filepath of the document.
        url (Optional[str]): The url of the document.
        metadata (Optional[Dict]): The metadata of the document.    
    """

    content: str
    id: Optional[str] = None
    title: Optional[str] = None
    filepath: Optional[str] = None
    url: Optional[str] = None
    embedding_metadata: Optional[Dict] = None