import os
from typing import Generator, List, Optional, Tuple
from .document import Document
from .chunking_result import ChunkingResult
from .token_estimator import TokenEstimator
from langchain.text_splitter import MarkdownTextSplitter, RecursiveCharacterTextSplitter, PythonCodeTextSplitter

class UnsupportedFormatError(Exception):
    """Exception raised when a format is not supported."""
    pass


class TextChunker():
    """Text chunker class.

    Attributes:
        supported_formats (List[str]): List of supported file formats.
    """
    FILE_FORMAT_DICT = {
        "md": "markdown",
        "txt": "text",
        "html": "html",
        "shtml": "html",
        "htm": "html",
        "py": "python",
        "pdf": "pdf",
    }
    SENTENCE_ENDINGS = [".", "!", "?"]
    WORDS_BREAKS = ['\n', '\t', '}', '{', ']', '[', ')', '(', ' ', ':', ';', ',']
    TOKEN_ESTIMATOR = TokenEstimator()

    def _get_file_format(self, file_path: str) -> Optional[str]:
        """Gets the file format from the file name.
        Returns None if the file format is not supported.
        Args:
            file_path (str): The file path of the file whose format needs to be retrieved.
        Returns:
            str: The file format.
        """
        # in case the caller gives us a file path
        file_path = os.path.basename(file_path)
        file_extension = file_path.split(".")[-1]
        return self.FILE_FORMAT_DICT.get(file_extension, None)

    def _chunk_content_helper(self,
            content: str, file_format: str, file_path: Optional[str],
            token_overlap: int,
            num_tokens: int
    ) -> Generator[Tuple[str, int, Document], None, None]:

        if file_format == "markdown":
            splitter = MarkdownTextSplitter.from_tiktoken_encoder(chunk_size=num_tokens, chunk_overlap=token_overlap)
        elif file_format == "python":
            splitter = PythonCodeTextSplitter.from_tiktoken_encoder(chunk_size=num_tokens, chunk_overlap=token_overlap)
        else:
            splitter = RecursiveCharacterTextSplitter.from_tiktoken_encoder(
                separators=self.SENTENCE_ENDINGS + self.WORDS_BREAKS,
                chunk_size=num_tokens, chunk_overlap=token_overlap)
        chunked_content_list = splitter.split_text(content)
        for chunked_content in chunked_content_list:
            chunk_size = self.TOKEN_ESTIMATOR.estimate_tokens(chunked_content)
            yield chunked_content, chunk_size, content


    def chunk_content(
        self,
        content: str,
        file_path: Optional[str] = None,
        url: Optional[str] = None,
        ignore_errors: bool = True,
        num_tokens: int = 2048,
        min_chunk_size: int = 10,
        token_overlap: int = 0
    ) -> ChunkingResult:
        """Chunks the given content. If ignore_errors is true, returns None
         in case of an error
        Args:
            content (str): The content to chunk.
            file_path (str): The file name. used for title, file format detection.
            url (str): The url. used for title.
            ignore_errors (bool): If true, ignores errors and returns None.
            num_tokens (int): The number of tokens in each chunk.
            min_chunk_size (int): The minimum chunk size below which chunks will be filtered.
            token_overlap (int): The number of tokens to overlap between chunks.
        Returns:
            List[Document]: List of chunked documents.
        """

        try:
            if file_path is None:
                file_format = "text"
            else:
                file_format = self._get_file_format(file_path)
                if file_format is None:
                    raise UnsupportedFormatError(
                        f"{file_path} is not supported")

            chunked_context = self._chunk_content_helper(
                content=content,
                file_path=file_path,
                file_format=file_format,
                num_tokens=num_tokens,
                token_overlap=token_overlap
            )
            chunks = []
            skipped_chunks = 0
            for chunk, chunk_size, doc in chunked_context:
                if chunk_size >= min_chunk_size:
                    chunks.append(
                        Document(
                            content=chunk,
                            title=file_path,
                            url=url,
                            filepath=file_path
                        )
                    )
                else:
                    skipped_chunks += 1

        except UnsupportedFormatError as e:
            if ignore_errors:
                return ChunkingResult(
                    chunks=[], total_files=1, num_unsupported_format_files=1
                )
            else:
                raise e
        except Exception as e:
            if ignore_errors:
                return ChunkingResult(chunks=[], total_files=1, num_files_with_errors=1)
            else:
                raise e
        return ChunkingResult(
            chunks=chunks,
            total_files=1,
            skipped_chunks=skipped_chunks,
        )