#
# The 'base' deployment provisions resources that are needed
# to support later stages
#

resource "azurerm_resource_group" "rg" {
  location = var.location
  name     = var.resource_group_name
  tags     = var.tags
}

resource "random_string" "name_suffix" {
  length  = 5
  special = false
  upper   = false
  number  = false
}

resource "azurerm_container_registry" "acr" {
  name                = "acr${random_string.name_suffix.result}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Basic"
  # For simplicity, this uses the admin user for authenticating
  # For production, consider other authentication options: https://docs.microsoft.com/en-us/azure/container-registry/container-registry-authentication
  admin_enabled = true
  tags          = var.tags
}

resource "azurerm_search_service" "search" {
  name                = "search-${random_string.name_suffix.result}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "standard"
  tags                = var.tags
  allowed_ips         = []

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_storage_account" "data" {
  name                     = "sa${random_string.name_suffix.result}"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "GRS"
  tags                     = var.tags
}

resource "azurerm_storage_container" "data" {
  name                  = "docs"
  storage_account_name  = azurerm_storage_account.data.name
  container_access_type = "private"
}

resource "azurerm_cognitive_account" "cognitive_services" {
  name                = "cognitive-${random_string.name_suffix.result}"
  custom_subdomain_name = "cognitive-${random_string.name_suffix.result}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  kind                = "OpenAI"
  sku_name            = "S0"
  tags                = var.tags
}

resource "azurerm_cognitive_deployment" "deployment" {
  name                 = "gpt4o"
  cognitive_account_id = azurerm_cognitive_account.cognitive_services.id

  sku {
    name = "GlobalStandard"
    capacity = 30
  }

  model {
    format  = "OpenAI"
    name    = "gpt-4o"
    version = "2024-08-06"
  }
}