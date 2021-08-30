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
    RESOURCE_GROUP                      = var.resource_group_name
    NUM_BEAMS                           = var.num_beams
    MAX_LENGTH                          = var.max_length
    KEY                                 = random_password.api_key.result
    SUMMARIZER_MODEL                    = "facebook/bart-large-cnn"
  }

  site_config {
    linux_fx_version  = "DOCKER|${var.container_registry}/text_summarization_extractor:${data.local_file.image_tag.content}"
    always_on         = "true"
    default_documents = []
    min_tls_version   = 1.2
    health_check_path = "/api/healthcheck"
  }

  identity {
    type = "SystemAssigned"
  }
}