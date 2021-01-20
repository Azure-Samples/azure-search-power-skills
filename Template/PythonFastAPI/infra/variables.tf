variable "app_service_sku" {
  description = "The SKU (size - cpu/mem) of the app plan hosting the container. See: https://azure.microsoft.com/en-us/pricing/details/app-service/linux/"
}

variable "docker_registry_url" {

}

variable "docker_registry_username" {

}

variable "docker_registry_password" {

}

variable "docker_image" {

}

variable "subscription_id" {

}

variable "aml_resource_group" {

}

variable "tenant_id" {

}

variable "sp_app_id" {

}

variable "sp_app_secret" {

}

variable "aml_location" {

}

variable "debug" {
  default = false
}
