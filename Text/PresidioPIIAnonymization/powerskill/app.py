from typing import Dict, List

import uvicorn
from dotenv import load_dotenv
from fastapi import FastAPI, Security, Depends, HTTPException
from fastapi.security.api_key import APIKeyHeader, APIKey
from objdict import ObjDict
from pydantic import BaseModel
from starlette.status import HTTP_400_BAD_REQUEST, HTTP_403_FORBIDDEN

from powerskill import Presidio

load_dotenv()
app = FastAPI()
presidio = Presidio()


class Values(BaseModel):
    values: List = []


class Value(Values):
    recordId: str
    data: Dict[str, str] = None


API_KEY = "KEY"  # os.environ['KEY']
API_KEY_NAME = "Ocp-Apim-Subscription-Key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=False)


async def get_api_key(
        api_key_header: str = Security(api_key_header),
):
    if api_key_header == API_KEY:
        return api_key_header
    else:
        raise HTTPException(
            status_code=HTTP_403_FORBIDDEN, detail="Key not present"
        )


@app.get('/api/healthcheck', status_code=200)
async def health():
    return 'Ready'


@app.post('/api/extraction')
def anonymize(values: Values, api_key: APIKey = Depends(get_api_key)):
    body = values.dict()
    if not body:
        return 'Expected text within body of request. No text found.', HTTP_400_BAD_REQUEST
    text = body['values'][0]['data']['text']
    anonymized_text = presidio.analyze_and_anonymize(text)
    return build_output_response(body, anonymized_text)


def build_output_response(inputs, anonymized_text):
    """

    :param inputs: The inputs gathered from the extraction process
    :param outputs: The outputs object - power skill output
    :return: The json response object
    """
    values = ObjDict()
    values.values = []

    anonymized_text_dict = ObjDict()
    anonymized_text_dict["text"] = anonymized_text

    errors = ''
    values.values.append({'recordId': inputs['values'][0]['recordId'],
                          "errors": errors,
                          "data": anonymized_text_dict,
                          "warnings": ""})

    return values


# Remove these two lines below for non-debug/production mode
if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=5000)
