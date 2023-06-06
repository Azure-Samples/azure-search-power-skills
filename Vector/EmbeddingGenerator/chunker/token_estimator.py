import tiktoken

class TokenEstimator():
    GPT2_TOKENIZER = tiktoken.get_encoding("gpt2")

    def estimate_tokens(self, text: str) -> int:
        return len(self.GPT2_TOKENIZER.encode(text))