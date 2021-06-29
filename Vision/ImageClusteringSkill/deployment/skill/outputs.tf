output "skill_api_key" {
  value     = random_password.api_key.result
  sensitive = true
}
output "skill_api_hostname" {
  value = azurerm_app_service.dockerapp.default_site_hostname
}
output "image_tag" {
  value = data.local_file.image_tag.content
}
