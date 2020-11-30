from typing import Dict, FrozenSet, List, Optional, Sequence, Set, Tuple, Union

from fastapi import FastAPI, Request, Security, Depends, HTTPException
from fastapi.security.api_key import APIKeyHeader, APIKey
from pydantic import BaseModel
from dotenv import load_dotenv
from powerskill import powerskill
import os
from starlette.status import HTTP_403_FORBIDDEN


load_dotenv()
app = FastAPI()

class Values(BaseModel):
    values:List = []

class Value(Values):
    recordId: str
    data: Dict[str, str] = None


API_KEY = os.environ['KEY']
API_KEY_NAME = "Ocp-Apim-Subscription-Key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=False)

async def get_api_key(
    api_key_header: str = Security(api_key_header),
):

    if api_key_header == API_KEY:
        return api_key_header
    else:
        raise HTTPException(
            status_code=HTTP_403_FORBIDDEN, detail="CogSvc Key not present"
        )


@app.post('/api/extraction')
def extract(values: Values, api_key: APIKey = Depends(get_api_key)):
    body = values.dict()
    if not body:
        return 'Expected text within body of request. No text found.', status.HTTP_400_BAD_REQUEST
    else:
        return powerskill.go_extract(body)
