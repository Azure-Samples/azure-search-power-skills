# azurerm provider can be uncommented for independent deployment of this resource during dev
# provider "azurerm" {
#   features {}
# }

resource "azurerm_app_service" "dockerapp" {
  location            = var.location
  resource_group_name = var.resource_group

  name                = "classifierpowerskill${var.resource_suffix}"
  app_service_plan_id = var.app_service_plan_id

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
    WEBSITES_PORT                       = 5000
    DOCKER_REGISTRY_SERVER_URL          = var.docker_registry_url
    DOCKER_REGISTRY_SERVER_USERNAME     = var.docker_registry_username
    DOCKER_REGISTRY_SERVER_PASSWORD     = var.docker_registry_password
    DEBUG                               = var.debug
    RESOURCE_GROUP                      = var.resource_group
    AZUREML_MODEL_DIR                   = var.azureml_model_dir
    GET_LATEST_MODEL                    = var.get_latest_model
    EXPERIMENT_NAME                     = var.experiment_name
    KEY                                 = var.powerskill_api_key
  }

  site_config {
    linux_fx_version  = "DOCKER|${var.docker_image}"
    always_on         = "true"
    min_tls_version   = 1.2
    health_check_path = "/api/healthcheck"
  }

  identity {
    type = "SystemAssigned"
  }
}
