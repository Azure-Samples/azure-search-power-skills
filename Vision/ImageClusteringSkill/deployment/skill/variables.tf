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

variable "dbscan_model" {
  default = "books.pkl"
}
variable "api_debug" {
  default = "False"
}
variable "cluster_labels" {
  default = ""
}
