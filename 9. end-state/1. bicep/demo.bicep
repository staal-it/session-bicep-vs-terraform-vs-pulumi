param location string = resourceGroup().location

resource stg 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: 'sterwinbiceptst'
  location: location

  sku: {
    name: 'Standard_GZRS'
  }
  kind: 'StorageV2'
}

output storageAccountName string = stg.name
