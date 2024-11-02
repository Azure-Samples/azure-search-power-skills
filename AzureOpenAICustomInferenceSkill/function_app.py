import azure.functions as func
import json
import logging
import aiohttp
import asyncio
import os
from typing import Dict, Any, List
from dataclasses import dataclass
from enum import Enum
import base64
import time
from functools import lru_cache

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class ScenarioType(Enum):
    SUMMARIZATION = "summarization"
    ENTITY_RECOGNITION = "entity-recognition"
    IMAGE_CAPTIONING = "image-captioning"

@dataclass
class ModelConfig:
    temperature: float = 0.7
    top_p: float = 0.95
    max_tokens: int = 4096
    timeout: int = 30  # seconds
    retry_attempts: int = 3
    retry_delay: float = 0.5

class CustomSkillException(Exception):
    def __init__(self, message: str, status_code: int = 500):
        self.message = message
        self.status_code = status_code
        super().__init__(self.message)

app = func.FunctionApp()

@lru_cache(maxsize=1)
def load_custom_prompts() -> Dict[str, str]:
    """Load and cache custom prompts from JSON file"""
    try:
        with open('custom_prompts.json', 'r') as file:
            return json.load(file)
    except Exception as e:
        logger.error(f"Failed to load custom prompts: {e}")
        raise CustomSkillException("Failed to load custom prompts", 500)

def validate_environment() -> tuple[str, str]:
    """Validate required environment variables"""
    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
    endpoint = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
    
    if not api_key or not endpoint:
        raise CustomSkillException("Missing required environment variables", 500)
    
    return api_key, endpoint

async def call_azure_openai(session: aiohttp.ClientSession, 
                           endpoint: str,
                           headers: Dict[str, str],
                           payload: Dict[str, Any],
                           config: ModelConfig) -> Dict[str, Any]:
    """Make HTTP request to Azure OpenAI with retry logic"""
    for attempt in range(config.retry_attempts):
        try:
            async with asyncio.timeout(config.timeout):
                async with session.post(endpoint, headers=headers, json=payload) as response:
                    if response.status == 429:  # Rate limit
                        retry_after = float(response.headers.get('Retry-After', config.retry_delay))
                        await asyncio.sleep(retry_after)
                        continue
                        
                    response.raise_for_status()
                    return await response.json()
                    
        except asyncio.TimeoutError:
            if attempt == config.retry_attempts - 1:
                raise CustomSkillException("Request timeout", 408)
            await asyncio.sleep(config.retry_delay * (attempt + 1))
            
        except aiohttp.ClientError as e:
            if attempt == config.retry_attempts - 1:
                raise CustomSkillException(f"Request failed: {str(e)}", 500)
            await asyncio.sleep(config.retry_delay * (attempt + 1))
    
    raise CustomSkillException("Max retry attempts reached", 500)

def prepare_messages(request_body: Dict[str, Any], scenario: str,
                    custom_prompts: Dict[str, str]) -> List[Dict[str, Any]]:
    """Prepare messages based on scenario"""
    try:
        if scenario == ScenarioType.SUMMARIZATION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for summarization", 400)
                
            return [
                {"role": "system", "content": custom_prompts.get("summarize-default-system-prompt")},
                {"role": "user", "content": text}
            ]
            
        elif scenario == ScenarioType.ENTITY_RECOGNITION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for entity recognition", 400)
                
            return [
                {"role": "system", "content": custom_prompts.get("entity-recognition-default-system-prompt")},
                {"role": "user", "content": text}
            ]
            
        elif scenario == ScenarioType.IMAGE_CAPTIONING.value:
            raw_image_data = request_body.get("data", {}).get("image", {})
            if not raw_image_data:
                raise CustomSkillException("Missing image data", 400)
                
            image_data = raw_image_data.get("data")
            image_type = raw_image_data.get("contentType")
            
            if not image_data or not image_type:
                raise CustomSkillException("Invalid image data format", 400)

            # Ensure base64 encoding compatibility
            try:
                base64.b64decode(image_data)
            except Exception as e:
                raise CustomSkillException("Invalid base64 encoding", 400)
                
            image_base64encoded = f"data:{image_type};base64,{image_data}"
            
            return [
                {"role": "system", "content": custom_prompts.get("image-captioning-simple-description-prompt")},
                {"role": "user", "content": [
                    {
                        "type": "image_url",
                        "image_url": {"url": image_base64encoded}
                    },
                    {
                        "type": "text",
                        "text": "Please provide a caption for this image."
                    }
                ]}
            ]
            
        else:
            raise CustomSkillException(f"Unknown scenario: {scenario}", 400)
            
    except CustomSkillException:
        raise
    except Exception as e:
        logger.error(f"Error preparing messages: {e}")
        raise CustomSkillException(f"Failed to prepare messages: {str(e)}", 500)

def format_response(request_body: Dict[str, Any], response_text: str,
                   scenario: str) -> Dict[str, Any]:
    """Format response based on scenario"""
    try:
        response_body = {
            "recordId": request_body.get("recordId"),
            "warnings": None,
            "errors": [],
            "data": {}
        }
        
        if scenario == ScenarioType.SUMMARIZATION.value:
            response_body["data"] = {"generative-summary": response_text}
        elif scenario == ScenarioType.ENTITY_RECOGNITION.value:
            entities = [entity.strip() for entity in response_text.strip('[]').split(',')]
            response_body["data"] = {"entities": entities}
        elif scenario == ScenarioType.IMAGE_CAPTIONING.value:
            response_body["data"] = {"generative-caption": response_text}
            
        return response_body
    except Exception as e:
        logger.error(f"Error formatting response: {e}")
        return {
            "recordId": request_body.get("recordId"),
            "errors": [str(e)],
            "warnings": None,
            "data": None
        }

@app.function_name(name="AOAIHealthCheck")
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
async def health_check(req: func.HttpRequest) -> func.HttpResponse:
    """Enhanced health check endpoint"""
    try:
        api_key, endpoint = validate_environment()
        custom_prompts = load_custom_prompts()
        
        response_body = {
            "status": "Healthy",
            "timestamp": time.time(),
            "checks": {
                "environment": "OK",
                "prompts": "OK"
            }
        }
        return func.HttpResponse(json.dumps(response_body), mimetype="application/json")
    except Exception as e:
        return func.HttpResponse(
            json.dumps({"status": "Unhealthy", "error": str(e)}),
            mimetype="application/json",
            status_code=500
        )

@app.function_name(name="AOAICustomSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
async def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    """Main custom skill endpoint with improved error handling and performance"""
    start_time = time.time()
    
    try:
        # Validate request
        request_json = req.get_json()
        scenario = req.headers.get("scenario")
        if not scenario:
            raise CustomSkillException("Missing scenario in headers", 400)
            
        input_values = request_json.get("values", [])
        if not input_values:
            raise CustomSkillException("Missing 'values' in request body", 400)

        # Initialize configurations
        api_key, endpoint = validate_environment()
        custom_prompts = load_custom_prompts()
        config = ModelConfig()
        
        headers = {
            "Content-Type": "application/json",
            "api-key": api_key
        }

        async with aiohttp.ClientSession() as session:
            response_values = []
            for request_body in input_values:
                try:
                    # Prepare messages
                    messages = prepare_messages(request_body, scenario, custom_prompts)
                    
                    # Prepare request payload
                    request_payload = {
                        "messages": messages,
                        "temperature": config.temperature,
                        "top_p": config.top_p,
                        "max_tokens": config.max_tokens
                    }
                    
                    # Call Azure OpenAI
                    response_json = await call_azure_openai(
                        session=session,
                        endpoint=endpoint,
                        headers=headers,
                        payload=request_payload,
                        config=config
                    )
                    
                    response_text = response_json['choices'][0]['message']['content']
                    response_values.append(format_response(request_body, response_text, scenario))
                    
                except Exception as e:
                    logger.error(f"Error processing record {request_body.get('recordId')}: {e}")
                    response_values.append({
                        "recordId": request_body.get("recordId"),
                        "errors": [str(e)],
                        "warnings": None,
                        "data": None
                    })

        # Log processing time
        processing_time = time.time() - start_time
        logger.info(f"Processed {len(input_values)} records in {processing_time:.2f} seconds")
        
        # Return response
        return func.HttpResponse(
            json.dumps({"values": response_values}),
            mimetype="application/json"
        )
        
    except CustomSkillException as e:
        logger.error(f"Custom skill error: {e}")
        return func.HttpResponse(str(e), status_code=e.status_code)
    except json.JSONDecodeError:
        logger.error("Invalid JSON in request body")
        return func.HttpResponse("Invalid JSON in request body", status_code=400)
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return func.HttpResponse("Internal server error", status_code=500)
