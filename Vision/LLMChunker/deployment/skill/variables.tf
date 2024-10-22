variable "resource_group_name" {
}

variable "location" {
}

variable "tags" {
  default = {}
}

variable "subscription_id" {
  
}

variable "container_registry" {
}
variable "container_registry_admin_username" {
}
variable "container_registry_admin_password" {
}

variable "openai_url" {
}

variable "api_debug" {
  default = "false"
}

variable "openai_deployment" {
  description = "The deployment name of the OpenAI instance. Eg: gpt4o-deployment"
  default = "gpt4o"
}

variable "openai_api_version" {
  description = "The API version of Azure OpenAI according to https://learn.microsoft.com/azure/ai-services/openai/reference"
  default = "2024-06-01"
}

variable "openai_max_concurrent_requests" {
  description = "The maximum number of concurrent requests to make to the OpenAI at a given time. Increase or decrease this based in your available quota"
  default = "5"
}

variable "openai_max_images_per_request" {
  description = "The maximum number of images (document pages) to send to OpenAI in a single request. This parameter is highly dependent on the size of the images (which is set by the 'IMAGE_QUALITY' parameter). The more images in the request, better context OpenAI has to generate coherence markdown output, but it also increases response time and token consumption"
  default = "15"
}

variable "openai_max_retries" {
  description= "The maximum number of retries to make to the OpenAI API in case of a rate limiting responses (HTTP status code 429)"
  default = "6"
}

variable "openai_max_backoff" {
  description="The maximum number of seconds to wait before retrying a request to the OpenAI API in case of a rate limiting responses (HTTP status code 429)"
  default = "60"
}

variable "openai_max_tokens_response" {
  description = "The maximum number of tokens to expect in the OpenAI response per image. If your parameter 'openai_max_images_per_request' is set to 15 and 'openai_max_tokens_response' is set to 1024, that means your total max_tokens in the response will be 15 * 1024 = 15360. Do not set this value above 16000, which is GPT4o/mini max tokens response."
  default = "1024"
}

variable "image_quality" {
  description = "The quality of the images to be sent to OpenAI. The higher the quality, the more tokens it will consume. The available options are: low, high_720p, high_1024p, high_1920p. If your source documents have small text, increase this value until you get satisfactory results. To understand this parameter better and cost implications, go to https://platform.openai.com/docs/guides/vision/low-or-high-fidelity-image-understanding"
  default = "high_1024p"
}

variable "chunk_size" {
  description = "The size (in tokens) to split the markdown sections in case they go over this value. If a markdown section of heading 3 (###) goes over this value, it will be split into multiple chunks. If the markdown section is below that token size, it will be returned as a single chunk."
  default = "512"
}

variable "chunk_overlap" {
  description = "The percentage of overlap between chunks. This parameter is used to avoid splitting sentences between chunks."
  default = "25"
}