output "resource_group" {
  value = azurerm_resource_group.rg.name
}
output "location" {
  value = azurerm_resource_group.rg.location
}

output "azure_search_admin_key" {
  value = azurerm_search_service.search.primary_key
}
output "azure_search_query_key" {
  value = azurerm_search_service.search.query_keys[0].key
}
