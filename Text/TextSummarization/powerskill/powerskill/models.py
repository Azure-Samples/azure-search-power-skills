from transformers import BartTokenizer, BartForConditionalGeneration


class Models:

    def __init__(self, summarizer_model, tokenizer=None, model=None):
        self.tokenizer = tokenizer
        self.model = model

    def load_summarisation_model(self, summarizer_model):
        self.tokenizer = BartTokenizer.from_pretrained(summarizer_model)
        self.model = BartForConditionalGeneration.from_pretrained(summarizer_model)
