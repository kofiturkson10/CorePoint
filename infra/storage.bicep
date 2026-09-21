@description('Azure region for the resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Storage account name: 3-24 characters, lowercase letters and digits only, globally unique.')
@minLength(3)
@maxLength(24)
param storageAccountName string = 'stfp${uniqueString(resourceGroup().id)}'

@description('Name of the blob container that holds the documents.')
param documentsContainerName string = 'documents'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

resource documentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: documentsContainerName
  properties: {
    publicAccess: 'None'
  }
}

output storageAccountName string = storageAccount.name
output documentsContainerName string = documentsContainer.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob
