variable "app_service_sku" {
  description = "The SKU (size - cpu/mem) of the app plan hosting the container. See: https://azure.microsoft.com/en-us/pricing/details/app-service/linux/"
  default = "P2V2"
}

variable "docker_registry_url" {
  description = "[your container registry].azurecr.io"
  default = ""
}

variable "docker_registry_username" {
  description = "[your container registry username]"
  default = ""
}

variable "docker_registry_password" {
  description = "[your container registry password]"
  default = ""
}

variable "docker_image" {
  description = "[your docker image name]:[your tag]"
  default = ""
}

variable "dbscan_model" {
  description = "Set this to books.pkl (if using demo value)"
  default = "books.pkl"
}

variable "resource_group" {
  description = "This is the name of an existing resource group to deploy to"
  default = ""
}

variable "location" {
  description = "This is the region of an existing resource group you want to deploy to"
  default = "eastus2"
}

variable "debug" {
  description = "API logging - set to True for verbose logging"
  default = false
}

variable "cluster_labels" {
  description = "Set this to labels.pkl (if using demo value)"
  default = "labels.pkl"
}

variable "api_key" {
  description = "The api key to assign to the skill API, i.e. the key value that callers must present when calling the API"
  default = "[YourSecretKeyCanBeAnything]"
}
