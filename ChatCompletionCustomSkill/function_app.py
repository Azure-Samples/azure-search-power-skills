import azure.functions as func
import json
import logging
import os
import asyncio
from azure.ai.inference.aio import ChatCompletionsClient
from azure.core.credentials import AzureKeyCredential
from typing import Dict, Any, List
from dataclasses import dataclass
from enum import Enum
import base64
import time
import requests
from azure.ai.inference.models import (
        SystemMessage,
        UserMessage,
    )

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

class CustomSkillException(Exception):
    def __init__(self, message: str, status_code: int = 500):
        self.message = message
        self.status_code = status_code
        super().__init__(self.message)

app = func.FunctionApp()

def load_custom_prompts() -> Dict[str, str]:
    """Load custom prompts from JSON file"""
    try:
        with open('custom_prompts.json', 'r') as file:
            return json.load(file)
    except Exception as e:
        logger.error(f"Failed to load custom prompts: {e}")
        raise CustomSkillException("Failed to load custom prompts", 500)

async def create_chat_client() -> ChatCompletionsClient:
    """Create and configure chat client"""
    try:
        api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
        endpoint = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
        
        if not api_key or not endpoint:
            raise CustomSkillException("Missing required environment variables", 500)
            
        print(f"Creating chat client with endpoint: {endpoint} and api_key: {api_key}")
        return ChatCompletionsClient(
            endpoint=endpoint,
            credential=AzureKeyCredential(api_key)
        )
    except Exception as e:
        logger.error(f"Failed to create chat client: {e}")
        raise CustomSkillException("Failed to initialize chat client", 500)

def prepare_messages(request_body: Dict[str, Any], scenario: str, 
                    custom_prompts: Dict[str, str]) -> List[Dict[str, Any]]:
    """Prepare messages based on scenario"""
    try:
        if scenario == ScenarioType.SUMMARIZATION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for summarization", 400)
            # system_message = SystemMessage(content=custom_prompts.get("summarize-default-system-prompt"))
            system_message = {
            "role": "system",
            "content": [
                    {
                        "type": "text",
                        "text": custom_prompts.get("summarize-default-system-prompt")
                    }
                ]
            }
            # user_message = UserMessage(content=text)
            user_message = {
            "role": "user",
            "content": [
                    {
                    "type": "text",
                    "text": text
                    }
                ]
            }
            return [ system_message, user_message]
            
        elif scenario == ScenarioType.ENTITY_RECOGNITION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for entity recognition", 400)
            system_message = SystemMessage(content=custom_prompts.get("entity-recognition-default-system-prompt"))
            user_message = UserMessage(content=text)
            return [ system_message, user_message ]
            
        elif scenario == ScenarioType.IMAGE_CAPTIONING.value:
            raw_image_data = request_body.get("data", {}).get("image", {})
            if not raw_image_data:
                raise CustomSkillException("Missing image data", 400)
                
            image_data = raw_image_data.get("data")
            image_type = raw_image_data.get("contentType")
            
            if not image_data or not image_type:
                raise CustomSkillException("Invalid image data format", 400)
                
            # Validate base64 content
            try:
                base64.b64decode(image_data)
            except Exception:
                raise CustomSkillException("Invalid base64 encoding", 400)
                
            image_base64encoded = f"data:{image_type};base64,{image_data}"
            system_message = SystemMessage(content=custom_prompts.get("image-captioning-default-system-prompt"))
            return [
                system_message,
                {
                    "role": "user",
                    "content": [
                        {
                            "type": "image_url",
                            "image_url": {"url": image_base64encoded}
                        },
                        {
                            "type": "text",
                            "text": "I want you to describe this image in a few simple sentences. If there are people or places in the image that you recognize, please mention them."
                        }
                    ]
                }
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

@app.function_name(name="AIStudioModelCatalogHealthCheck")
@app.route(route="health", auth_level=func.AuthLevel.ANONYMOUS)
async def health_check(req: func.HttpRequest) -> func.HttpResponse:
    """Health check endpoint"""
    try:
        custom_prompts = load_custom_prompts()
        async with await create_chat_client() as client:
            response_body = {
                "status": "Healthy",
                "timestamp": time.time(),
                "checks": {
                    "prompts": "OK",
                    "client": "OK"
                }
            }
            return func.HttpResponse(json.dumps(response_body), mimetype="application/json")
    except Exception as e:
        return func.HttpResponse(
            json.dumps({"status": "Unhealthy", "error": str(e)}),
            mimetype="application/json",
            status_code=500
        )

@app.function_name(name="AIStudioModelCatalogSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
async def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    """Main custom skill endpoint"""
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

        # Load prompts and create client
        custom_prompts = load_custom_prompts()
        config = ModelConfig()
        
        response_values = []
        api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
        endpoint = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
        headers = {
        "Content-Type": "application/json",
        "api-key": api_key,
        "Authorization": f"Bearer {api_key}"
        }
        async with await create_chat_client() as client:
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

                    # print(f'request_payload: {request_payload}')
                    print(f'The headers are : {headers}')
                    vanilla_response = requests.post(endpoint, headers=headers, json=request_payload)
                    print(f'the vanilla response is : {vanilla_response}')
                    vanilla_response.raise_for_status()  # Will raise an HTTPError if the HTTP request returned an unsuccessful status code
                    vanilla_response_json = vanilla_response.json()
                    # TODO: this WORKED! Now I just need to massage this requests.post() response into the same format as the ChatCompletionsClient response
                    print(f'vanilla_response_json: {vanilla_response_json}')
                    
                    # Call model with timeout
                    async with asyncio.timeout(config.timeout):
                        # just use requests.post() here instead
                        response = await client.complete(request_payload)
                        response_text = response.choices[0].message.content
                        
                    # Format response
                    response_values.append(format_response(request_body, response_text, scenario))
                    
                except asyncio.TimeoutError:
                    logger.error(f"Timeout processing record {request_body.get('recordId')}")
                    response_values.append({
                        "recordId": request_body.get("recordId"),
                        "errors": ["Request timeout"],
                        "warnings": None,
                        "data": None
                    })
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