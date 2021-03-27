import logging
import os

from dotenv import load_dotenv
from objdict import ObjDict
from powerskill.timer import timefunc

load_dotenv()


def set_log_level(debug):
    """
    :param debug: Boolean value
    :return: None
    """
    if bool(debug):
        logging.basicConfig(level=logging.DEBUG)


set_log_level(bool(os.environ['DEBUG']))


def build_output_response(inputs, outputs, summary, error=None):
    """

    :param inputs: The inputs gathered from the extraction process
    :param outputs: The outputs object - power skill output
    :return: The json response object
    """
    values = ObjDict()
    values.values = []

    summary_dict = ObjDict()
    errors = ''
    summary_dict["summary"] = summary
    values.values.append({'recordId': inputs['values'][0]['recordId'], \
                          "errors": errors,
                          "data": summary_dict,
                          "warnings": ""})

    return values


@timefunc
def go_extract(inputs, summarizer_model, tokenizer, max_length, num_beams):
    """
    :param args: inputs from web request
    :param classification_model: Our trained summarizer model
    :return: Summarized text
    """

    try:
        outputs = {}
        output_response = {}
        summary = ""

        logging.info(f"Inputs {inputs['values'][0]}")
        record_id = inputs['values'][0]['recordId']
        text = inputs['values'][0]['data']['text']

        # Call the sumarizer
        token_inputs = tokenizer([text], max_length=max_length, return_tensors='pt')
        summary_ids = summarizer_model.generate(token_inputs['input_ids'], num_beams=num_beams,
                                                max_length=max_length, early_stopping=True)
        summary = [tokenizer.decode(g, skip_special_tokens=True, clean_up_tokenization_spaces=False)
                   for g in summary_ids]

    except Exception as ProcessingError:
        logging.exception(ProcessingError)
        error = str(ProcessingError)
        output_response = build_output_response(inputs, outputs, summary)

    output_response = build_output_response(inputs, outputs, summary)
    logging.info(f"Output {output_response}")

    return output_response
