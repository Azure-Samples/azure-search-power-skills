import re
import sys

def simplify_punctuation(text):
    """
    This function simplifies doubled or more complex punctuation.
    The exception is '...'.
    """
    corrected = str(text)
    corrected = re.sub(r'([!?,;])\1+', r'\1', corrected)
    corrected = re.sub(r'\.{2,}', r'...', corrected)

    return corrected

def normalize_whitespace(text):
    """
    This function normalizes whitespaces, removing duplicates.
    """
    corrected = str(text)
    corrected = re.sub('\s', ' ', corrected)
    corrected = re.sub(r"( )\1+",r"\1", corrected)

    return corrected.strip(" ")

def truncate_text(text, maxLength=128):
    """
    This funtion truncated a text sequence to a maximum of "maxLength"
    whitespace separated tokens.
    """
    space_idx = [m.start() for m in re.finditer(' ', text)]
    if len(space_idx) >= maxLength:
        end_idx = space_idx[maxLength-1]
        text = (text[:end_idx])

    return text

def normalize_text(text):
    """
    This function applies puntuation and whitespace normalization,
    and returns the processed text in lowercase.
    """
    normalized_text = simplify_punctuation(text)
    normalized_text = normalize_whitespace(normalized_text)
    normalized_text = normalized_text.lower()
    return normalized_text

if __name__ == '__main__':
    text = sys.argv[1]
    text_norm = normalize_text(text)
    text_trunc = truncate_text(text_norm)  # Perform truncation only after duplicate whitespace has been processed

    print('Processed text:')
    print(text_trunc)