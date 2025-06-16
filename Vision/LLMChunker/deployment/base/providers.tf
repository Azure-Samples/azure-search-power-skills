terraform {
  required_version = ">= 0.13"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0.1"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = "${var.subscription_id}"
}
