import azure.functions as func
import json
import logging
import os
from typing import Dict, Any, List
from dataclasses import dataclass
from enum import Enum
import base64
import time
from functools import lru_cache
from openai import AzureOpenAI

# Load environment variables

# Configure logging
logging.basicConfig(level=logging.DEBUG)  # Set to DEBUG for more verbosity
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
        logger.error(f"CustomSkillException raised: {self.message}")  # Log exception on creation
        super().__init__(self.message)

app = func.FunctionApp()

def validate_environment() -> tuple[str, str, str, str]:
    """Validate required environment variables."""
    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
    endpoint = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
    deployment_name = "gpt-4o-mini"
    api_version = "2024-08-01-preview"
    
    if not api_key or not endpoint or not deployment_name or not api_version:
        logger.error("Missing required environment variables.")
        raise CustomSkillException("Missing required environment variables", 500)
    
    logger.info("Environment variables validated successfully.")
    return api_key, endpoint, deployment_name, api_version

def prepare_messages(request_body: Dict[str, Any], scenario: str) -> List[Dict[str, Any]]:
    """Prepare messages based on scenario."""
    try:
        logger.debug(f"Preparing messages for scenario: {scenario}")
        if scenario == ScenarioType.SUMMARIZATION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for summarization", 400)
                
            return [
                {"role": "system", "content": "You are an expert summarizer. Succinctly summarize the following text into no more than 200 words.###"},
                {"role": "user", "content": text}
            ]
            
        elif scenario == ScenarioType.ENTITY_RECOGNITION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for entity recognition", 400)
                
            return [
                {"role": "system", "content": "You are an expert in extracting entities from text. Identify all the named entities (e.g., people, places, organizations, dates, etc.) in the text below. Return them in a comma-separated list.###"},
                {"role": "user", "content": text}
            ]
            
        elif scenario == ScenarioType.IMAGE_CAPTIONING.value:
            raw_image_data = request_body.get("data", {}).get("image", {})
            image_url = raw_image_data.get("url")
            image_data = raw_image_data.get("data")
            image_type = raw_image_data.get("contentType")
            
            if image_url:
                # If an image URL is provided, use it directly
                return [
                    {"role": "system", "content": "You are an AI assistant that provides detailed and accurate captions for images. Please review the image provided and generate a caption that best describes its content.###"},
                    {"role": "user", "content": [
                        {
                            "type": "image_url",
                            "image_url": {"url": image_url}
                        },
                        {
                            "type": "text",
                            "text": "Provide a descriptive caption for this image."
                        }
                    ]}
                ]
            elif image_data and image_type:
                # If base64 encoded image data is provided
                try:
                    base64.b64decode(image_data)
                except Exception as e:
                    logger.error(f"Invalid base64 encoding: {e}")
                    raise CustomSkillException("Invalid base64 encoding", 400)
                
                image_base64encoded = f"data:{image_type};base64,{image_data}"
                
                return [
                    {"role": "system", "content": "You are an AI assistant that provides detailed and accurate captions for images. Please review the image provided and generate a caption that best describes its content.###"},
                    {"role": "user", "content": [
                        {
                            "type": "image_url",
                            "image_url": {"url": image_base64encoded}
                        },
                        {
                            "type": "text",
                            "text": "Provide a descriptive caption for this image."
                        }
                    ]}
                ]
            else:
                raise CustomSkillException("Invalid image data format. Please provide either an image URL or base64 encoded image data.", 400)
            
        else:
            raise CustomSkillException(f"Unknown scenario: {scenario}", 400)
            
    except CustomSkillException:
        raise
    except Exception as e:
        logger.error(f"Error preparing messages: {e}")
        raise CustomSkillException(f"Failed to prepare messages: {str(e)}", 500)

def format_response(request_body: Dict[str, Any], response_text: str,
                    scenario: str) -> Dict[str, Any]:
    """Format response based on scenario."""
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
            
        logger.debug(f"Formatted response: {response_body}")
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
    """Enhanced health check endpoint."""
    try:
        api_key, endpoint, deployment_name, api_version = validate_environment()
        
        response_body = {
            "status": "Healthy",
            "timestamp": time.time(),
            "checks": {
                "environment": "OK"
            }
        }
        logger.info("Health check successful.")
        return func.HttpResponse(json.dumps(response_body), mimetype="application/json")
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        return func.HttpResponse(
            json.dumps({"status": "Unhealthy", "error": str(e)}),
            mimetype="application/json",
            status_code=500
        )

@app.function_name(name="AOAICustomSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
async def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    """Main custom skill endpoint with improved error handling and performance."""
    start_time = time.time()
    
    try:
        # Validate request
        request_json = req.get_json()
        logger.debug(f"Received request JSON: {request_json}")
        scenario = req.headers.get("scenario")
        if not scenario:
            logger.error("Missing scenario in headers.")
            raise CustomSkillException("Missing scenario in headers", 400)
            
        input_values = request_json.get("values", [])
        if not input_values:
            logger.error("Missing 'values' in request body.")
            raise CustomSkillException("Missing 'values' in request body", 400)

        # Initialize configurations
        api_key, endpoint, deployment_name, api_version = validate_environment()
        config = ModelConfig()

        # Set up the Azure OpenAI client
        client = AzureOpenAI(
            api_key=api_key,
            azure_endpoint=endpoint,
            api_version=api_version
        )

        response_values = []
        for request_body in input_values:
            try:
                # Prepare messages
                messages = prepare_messages(request_body, scenario)
                
                # Call Azure OpenAI
                response = client.chat.completions.create(
                    model=deployment_name,
                    messages=messages,
                    temperature=config.temperature,
                    top_p=config.top_p,
                    max_tokens=config.max_tokens
                )
                
                response_text = response.choices[0].message.content
                logger.debug(f"Response text received: {response_text}")
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
        logger.info(f"Processed {len(input_values)} records in {processing_time:.2f} seconds.")
        
        # Return response
        return func.HttpResponse(
            json.dumps({"values": response_values}),
            mimetype="application/json"
        )
        
    except CustomSkillException as e:
        logger.error(f"Custom skill error: {e}")
        return func.HttpResponse(str(e), status_code=e.status_code)
    except json.JSONDecodeError:
        logger.error("Invalid JSON in request body.")
        return func.HttpResponse("Invalid JSON in request body", status_code=400)
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return func.HttpResponse("Internal server error", status_code=500)
