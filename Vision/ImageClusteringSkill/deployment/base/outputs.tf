output "azure_search_admin_key" {
  value     = azurerm_search_service.search.primary_key
  sensitive = true
}
output "azure_search_query_key" {
  value     = azurerm_search_service.search.query_keys[0].key
  sensitive = true
}
output "azure_search_name" {
  value = azurerm_search_service.search.name
}

output "storage_connection_string" {
  value     = azurerm_storage_account.data.primary_connection_string
  sensitive = true
}
output "storage_account_name" {
  value = azurerm_storage_account.data.name
}
output "storage_container_name" {
  value = azurerm_storage_container.books.name
}

output "container_registry" {
  value = azurerm_container_registry.acr.login_server
}

# For simplicity, this uses the admin user for authenticating
# For production, consider other authentication options: https://docs.microsoft.com/en-us/azure/container-registry/container-registry-authentication
output "container_registry_admin_username" {
  value     = azurerm_container_registry.acr.admin_username
  sensitive = true
}
output "container_registry_admin_password" {
  value     = azurerm_container_registry.acr.admin_password
  sensitive = true
}

output "cognitive_services_key" {
  value     = azurerm_cognitive_account.cognitive_services.primary_access_key
  sensitive = true
}
