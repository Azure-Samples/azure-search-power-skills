terraform {
  backend "azurerm" {}
  required_version = ">= 0.13"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 2.30"
    }
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "powerskill" {
  location = var.aml_location
  name     = var.aml_resource_group
}

resource "azurerm_app_service_plan" "appserviceplan" {
  location            = var.aml_location
  resource_group_name = var.aml_resource_group

  name     = "mlappserviceplan"
  kind     = "Linux"
  reserved = true

  sku {
    tier = "Standard"
    size = "P1V2"
  }
}

resource "random_string" "random" {
  length  = 5
  special = false
  upper   = false
  number  = false
}

resource "azurerm_app_service" "dockerapp" {
  location            = var.aml_location
  resource_group_name = var.aml_resource_group

  name                = "extraction${random_string.random.result}"
  app_service_plan_id = azurerm_app_service_plan.appserviceplan.id

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
    WEBSITES_PORT                       = 5000
    DOCKER_REGISTRY_SERVER_URL          = var.docker_registry_url
    DOCKER_REGISTRY_SERVER_USERNAME     = var.docker_registry_username
    DOCKER_REGISTRY_SERVER_PASSWORD     = var.docker_registry_password
    DEBUG                               = var.debug
    RESOURCE_GROUP                      = var.aml_resource_group
    SUBSCRIPTION_ID                     = var.subscription_id
    TENANT_ID                           = var.tenant_id
    SP_APP_ID                           = var.sp_app_id
    SP_APP_SECRET                       = var.sp_app_secret
  }

  site_config {
    linux_fx_version = "DOCKER|${var.docker_image}"
    always_on        = "true"
  }

  identity {
    type = "SystemAssigned"
  }
}
