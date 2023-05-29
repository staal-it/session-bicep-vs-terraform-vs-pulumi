terraform {
  required_version = ">= 0.12.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 3.2.0"
    }
  }
}

provider "azurerm" {
  features {}
}

variable "location" {
  type    = string
  default = "westeurope"
}

resource "azurerm_storage_account" "erwin" {
  name                     = "sterwinterratst"
  resource_group_name      = "rg-terraform-tst2"
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

output "name" {
  value = azurerm_storage_account.erwin.name
}
