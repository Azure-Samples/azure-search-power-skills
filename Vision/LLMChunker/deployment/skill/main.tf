resource "random_string" "name_suffix" {
  length  = 5
  special = false
  upper   = false
  number  = false
}

resource "random_password" "api_key" {
  length  = 16
  special = true
}

data "local_file" "image_tag" {
  filename = "../outputs/image_tag.txt"
}

data "azurerm_resource_group" "rg" {
    name = var.resource_group_name
}

resource "azurerm_app_service_plan" "appserviceplan" {
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  name     = "appserviceplan-${random_string.name_suffix.result}"
  kind     = "Linux"
  reserved = true

  sku {
    tier = "PremiumV2"
    size = "P2v2"
  }
}

resource "azurerm_app_service" "dockerapp" {
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  name                = "extraction${random_string.name_suffix.result}"
  app_service_plan_id = azurerm_app_service_plan.appserviceplan.id

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
    WEBSITES_PORT                       = 5000
    DOCKER_REGISTRY_SERVER_URL          = "https://${var.container_registry}"
    DOCKER_REGISTRY_SERVER_USERNAME     = var.container_registry_admin_username
    DOCKER_REGISTRY_SERVER_PASSWORD     = var.container_registry_admin_password
    DEBUG                               = var.api_debug
    OPENAI_URL = var.openai_url
    OPENAI_DEPLOYMENT = "gpt4o"
    OPENAI_API_VERSION = "2024-06-01"
    OPENAI_MAX_CONCURRENT_REQUESTS = 5
    OPENAI_MAX_IMAGES_PER_REQUEST = 15
    OPENAI_MAX_RETRIES = 6
    OPENAI_MAX_BACKOFF = 60
    OPENAI_MAX_TOKENS_RESPONSE = 1024
    IMAGE_QUALITY = "high_1024p"
    CHUNK_SIZE = 512
    CHUNK_OVERLAP = 25
    KEY  = random_password.api_key.result
  }

  site_config {
    linux_fx_version  = "DOCKER|${var.container_registry}/llm_chunker:${data.local_file.image_tag.content}"
    always_on         = "true"
    default_documents = []
    min_tls_version   = 1.2
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "openai_user_role" {
    principal_id = azurerm_app_service.dockerapp.identity[0].principal_id
    role_definition_name = "Cognitive Services OpenAI User"
    scope = data.azurerm_resource_group.rg.id
}

resource "azurerm_role_assignment" "blob_reader_role" {
    principal_id = azurerm_app_service.dockerapp.identity[0].principal_id
    role_definition_name = "Storage Blob Data Reader"
    scope = data.azurerm_resource_group.rg.id
}