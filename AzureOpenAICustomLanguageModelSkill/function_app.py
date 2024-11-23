import azure.functions as func
import json
import logging
import os
from typing import Dict, Any, List
from dataclasses import dataclass
from enum import Enum
import base64
import time
from openai import AzureOpenAI
from pydantic import BaseModel, Field
from azure.ai.inference.models import (
        SystemMessage,
        UserMessage,
    )

# Configure logging
logging.basicConfig(level=logging.DEBUG)
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
    timeout: int = 30
    retry_attempts: int = 3
    retry_delay: float = 0.5


# Pydantic models for structured outputs
class SummaryResponse(BaseModel):
    summary: str = Field(
        description="A concise summary of the input text, no more than 200 words"
    )


class Entity(BaseModel):
    name: str = Field(description="The name of the identified entity")
    type: str = Field(
        description="The type/category of the entity (e.g., PERSON, ORGANIZATION, LOCATION)"
    )
    confidence: float = Field(description="Confidence score for the entity")


class EntityResponse(BaseModel):
    entities: List[Entity] = Field(
        description="List of extracted entities from the text"
    )


class ImageCaptionResponse(BaseModel):
    caption: str = Field(description="A detailed description of the image content")
    tags: List[str] = Field(
        description="List of relevant tags or keywords from the image"
    )
    confidence: float = Field(description="Overall confidence score for the caption")


class CustomSkillException(Exception):
    def __init__(self, message: str, status_code: int = 500):
        self.message = message
        self.status_code = status_code
        logger.error(f"CustomSkillException raised: {self.message}")
        super().__init__(self.message)


app = func.FunctionApp()


def validate_environment() -> tuple[str, str, str, str]:
    """Validate required environment variables."""
    api_key = os.getenv("AZURE_INFERENCE_CREDENTIAL")
    endpoint = os.getenv("AZURE_CHAT_COMPLETION_ENDPOINT")
    deployment_name = "gpt-4o"  # Make sure this is your GPT-4o deployment name
    api_version = "2024-08-01-preview"

    if not api_key or not endpoint or not deployment_name or not api_version:
        logger.error("Missing required environment variables.")
        raise CustomSkillException("Missing required environment variables", 500)

    return api_key, endpoint, deployment_name, api_version


def prepare_messages(
    request_body: Dict[str, Any], scenario: str
) -> List[Dict[str, Any]]:
    """Prepare messages based on scenario."""
    try:
        if scenario == ScenarioType.SUMMARIZATION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for summarization", 400)
            system_message = SystemMessage(content="You are an expert summarizer. Create a structured summary of the following text.")
            user_message = UserMessage(content=text)
            return [ system_message, user_message ]

        elif scenario == ScenarioType.ENTITY_RECOGNITION.value:
            text = request_body.get("data", {}).get("text", "")
            if not text:
                raise CustomSkillException("Missing text for entity recognition", 400)
            system_message = SystemMessage(content="Extract and classify all named entities from the text with confidence scores.")
            user_message = UserMessage(content=text)
            return [ system_message, user_message ]

        elif scenario == ScenarioType.IMAGE_CAPTIONING.value:
            raw_image_data = request_body.get("data", {}).get("image", {})
            image_url = raw_image_data.get("url")
            image_data = raw_image_data.get("data")
            image_type = raw_image_data.get("contentType")
            system_image_verbalization_message = "You are an AI assistant that provides structured image descriptions with tags and confidence scores."
            if image_url:
                return [
                    {"role": "system", "content": system_image_verbalization_message},
                    {
                        "role": "user",
                        "content": [
                            {"type": "image_url", "image_url": {"url": image_url}},
                            {
                                "type": "text",
                                "text": "Analyze this image and provide a structured description.",
                            },
                        ],
                    },
                ]
            elif image_data and image_type:
                try:
                    base64.b64decode(image_data)
                except Exception as e:
                    raise CustomSkillException("Invalid base64 encoding", 400)

                image_base64encoded = f"data:{image_type};base64,{image_data}"
                return [
                    {"role": "system", "content": system_image_verbalization_message},
                    {
                        "role": "user",
                        "content": [
                            {
                                "type": "image_url",
                                "image_url": {"url": image_base64encoded},
                            },
                            {
                                "type": "text",
                                "text": "Analyze this image and provide a structured description.",
                            },
                        ],
                    },
                ]
            else:
                raise CustomSkillException("Invalid image data format", 400)
        else:
            raise CustomSkillException(f"Unknown scenario: {scenario}", 400)

    except CustomSkillException:
        raise
    except Exception as e:
        logger.error(f"Error preparing messages: {e}")
        raise CustomSkillException(f"Failed to prepare messages: {str(e)}", 500)


def format_response(
    request_body: Dict[str, Any], parsed_response: BaseModel
) -> Dict[str, Any]:
    """Format response using parsed structured output."""
    try:
        response_body = {
            "recordId": request_body.get("recordId"),
            "warnings": None,
            "errors": [],
            "data": {},
        }

        # Since we're using Pydantic models, we can access the fields directly
        if isinstance(parsed_response, SummaryResponse):
            response_body["data"] = {"generative-summary": parsed_response.summary}
        elif isinstance(parsed_response, EntityResponse):
            response_body["data"] = {
                "entities": [
                    {
                        "name": entity.name,
                        "type": entity.type,
                        "confidence": entity.confidence,
                    }
                    for entity in parsed_response.entities
                ]
            }
        elif isinstance(parsed_response, ImageCaptionResponse):
            response_body["data"] = {
                "generative-caption": parsed_response.caption,
                "tags": parsed_response.tags,
                "confidence": parsed_response.confidence,
            }

        return response_body
    except Exception as e:
        logger.error(f"Error formatting response: {e}")
        return {
            "recordId": request_body.get("recordId"),
            "errors": [str(e)],
            "warnings": None,
            "data": None,
        }


@app.function_name(name="AOAICustomSkill")
@app.route(route="custom_skill", auth_level=func.AuthLevel.ANONYMOUS)
async def custom_skill(req: func.HttpRequest) -> func.HttpResponse:
    """Main custom skill endpoint using structured outputs."""
    start_time = time.time()

    try:
        request_json = req.get_json()
        logger.debug(f"Received request JSON: {request_json}")
        scenario = req.headers.get("scenario")
        if not scenario:
            raise CustomSkillException("Missing scenario in headers", 400)

        input_values = request_json.get("values", [])
        if not input_values:
            raise CustomSkillException("Missing 'values' in request body", 400)

        api_key, endpoint, deployment_name, api_version = validate_environment()
        config = ModelConfig()

        client = AzureOpenAI(
            api_key=api_key, azure_endpoint=endpoint, api_version=api_version
        )

        response_values = []
        for request_body in input_values:
            try:
                messages = prepare_messages(request_body, scenario)

                # Select appropriate response model based on scenario
                response_model = {
                    ScenarioType.SUMMARIZATION.value: SummaryResponse,
                    ScenarioType.ENTITY_RECOGNITION.value: EntityResponse,
                    ScenarioType.IMAGE_CAPTIONING.value: ImageCaptionResponse,
                }[scenario]

                # Use parse method to get structured output
                completion = client.beta.chat.completions.parse(
                    model=deployment_name,
                    messages=messages,
                    response_format=response_model,
                    temperature=config.temperature,
                    top_p=config.top_p,
                    max_tokens=config.max_tokens,
                )

                parsed_response = completion.choices[0].message.parsed
                logger.debug(f"Parsed response: {parsed_response}")
                response_values.append(format_response(request_body, parsed_response))

            except Exception as e:
                logger.error(
                    f"Error processing record {request_body.get('recordId')}: {e}"
                )
                response_values.append(
                    {
                        "recordId": request_body.get("recordId"),
                        "errors": [str(e)],
                        "warnings": None,
                        "data": None,
                    }
                )

        processing_time = time.time() - start_time
        logger.info(
            f"Processed {len(input_values)} records in {processing_time:.2f} seconds."
        )

        return func.HttpResponse(
            json.dumps({"values": response_values}), mimetype="application/json"
        )

    except CustomSkillException as e:
        return func.HttpResponse(str(e), status_code=e.status_code)
    except json.JSONDecodeError:
        return func.HttpResponse("Invalid JSON in request body", status_code=400)
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return func.HttpResponse("Internal server error", status_code=500)


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
                "environment": "OK",
                "endpoint": endpoint,
                "deployment": deployment_name,
                "api_version": api_version,
                "api_key_present": bool(api_key)  # Don't expose the actual key
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