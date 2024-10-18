import os
from fastapi.security import APIKeyHeader
import uvicorn
from dotenv import load_dotenv
from fastapi import Depends, FastAPI, HTTPException, Response, Security
from fastapi.security.api_key import APIKeyHeader, APIKey
from skill import markdown_generator
from models.skill_input import RequestProcess
from models.skill_output import ResponseProcess
from utils.conversion_utils import ConversionUtils
from utils.validation_utils import ValidationUtils

load_dotenv()
app = FastAPI()

API_KEY = os.environ.get('KEY')
API_KEY_NAME = "api-key"

api_key_header = APIKeyHeader(name=API_KEY_NAME, auto_error=False)

async def get_api_key(
        api_key_header: str = Security(api_key_header),
):
    if api_key_header == API_KEY:
        return api_key_header
    else:
        if API_KEY is not None and API_KEY != "":
            raise HTTPException(status_code=403, detail="HTTP header 'api-key' is missing or invalid")
        
@app.get('/api/healthcheck', status_code=200)
async def healthcheck():
    return 'Ready'

@app.post('/process')
async def process(input: RequestProcess, response: Response, api_key: APIKey = Depends(get_api_key)) -> ResponseProcess:
    try:
        ValidationUtils.validate_process_request(input)
        responseData = await markdown_generator.process(input.values[0].data)
        return ConversionUtils.create_response_markdown(responseData)
    except Exception as ex:  
        responseError =  ConversionUtils.create_response_exception(ex)  
        response.status_code = 500
        return responseError
    
# Remove these two lines below for non-debug/production mode
if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=5000)
