import numpy as np
import sys

def get_ids_and_masks(tokenizer, text):
    """
    This function takes a BertWordPieceTokenizer "tokenizer" and string "text"
    as input and returns the input_ids and attention_masks computed for "text".
    """
    input_ids = []
    attention_masks = []

    encoding = tokenizer.encode(text)
    input_ids.append(encoding.ids)
    attention_masks.append(encoding.attention_mask)

    return input_ids, attention_masks

def predict(ort_session, input_ids, attention_masks):
    """
    This function takes an ONNX runtime session "ort_session", tokenized sequence
    "input_ids", and corresponding attention mask sequence "attention_masks" and
    returns the predicted class for the text. Class [0] indicates text of good quality.
    Class [1] suggests that the text quality could be bad.
    """
    input_dict = {"input_ids" : input_ids, "attention_mask" : attention_masks}
    outputs = ort_session.run(["logits"], input_dict)

    logits = outputs[0]
    flat_prediction = np.argmax(logits, axis=1).flatten()

    return flat_prediction

if __name__ == '__main__':
    tokenizer = sys.argv[1]
    ort_session = sys.argv[2]
    text = sys.argv[3]

    input_ids, attention_masks = get_ids_and_masks(tokenizer, text)
    flat_prediction = predict(ort_session, input_ids, attention_masks)

    print('Predicted class is: ' + str(flat_prediction))