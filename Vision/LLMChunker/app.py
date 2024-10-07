import uvicorn
from dotenv import load_dotenv
from fastapi import FastAPI, Response
from models.skill_input import RequestProcess
from models.skill_output import ResponseProcess
from powerskill import markdown_generator
from utils.validation_utils import ValidationUtils
from utils.conversion_utils import ConversionUtils

load_dotenv()
app = FastAPI()

@app.post('/process')
async def process(input: RequestProcess, response: Response) -> ResponseProcess:
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
