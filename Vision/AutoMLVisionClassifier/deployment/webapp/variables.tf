variable "app_service_sku" {
  description = "The SKU (size - cpu/mem) of the app plan hosting the container. See: https://azure.microsoft.com/en-us/pricing/details/app-service/linux/"
  default     = "P2V2"
}

variable "docker_registry_url" {
  description = "[your container registry].azurecr.io"
  default     = ""
}

variable "docker_registry_username" {
  description = "[your container registry username]"
  default     = ""
}

variable "docker_registry_password" {
  description = "[your container registry password]"
  default     = ""
}

variable "docker_image" {
  description = "[your docker image name]:[your tag]"
  default     = "shanepeckham/amlclassifier_powerskill:v1"
}

variable "resource_group" {
  description = "This is the name of an existing resource group to deploy to"
}

variable "location" {
  description = "This is the region of an existing resource group you want to deploy to"
}

variable "debug" {
  description = "API logging - set to True for verbose logging"
  default     = false
}

variable "azureml_model_dir" {
  description = "The model directory where the AML model resides"
  default     = "models"
}

variable "get_latest_model" {
  description = "If true, downloads the latest model from AML"
  default     = false
}

variable "experiment_name" {
  description = "The training experiment associated with the AML Labelling project"
  default     = "label_training_1"
}

variable "powerskill_api_key" {
  description = "The api key to use for auth with this power skill"
}

variable "app_service_plan_id" {
  description = "ID of the App Service Plan resource to deploy on"
}

variable "resource_suffix" {
  description = "Optional suffix to append to resource names to avoid naming collisions."
  default     = ""
}
