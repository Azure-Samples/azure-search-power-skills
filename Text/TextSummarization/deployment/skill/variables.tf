variable "resource_group_name" {
}

variable "location" {
}

variable "tags" {
  default = {}
}

variable "container_registry" {
}
variable "container_registry_admin_username" {
}
variable "container_registry_admin_password" {
}

variable "api_debug" {
  default = "false"
}

variable "num_beams" {
  description = "Set this to the number of beams to use for beam search"
  default = 4
}

variable "max_length" {
  description = "The maximum length of the summary"
  default = 1024
}