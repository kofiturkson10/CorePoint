@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Display name for the Azure AD admin of the SQL server, e.g. your work email. See sql.bicep - whoever runs this deployment becomes the admin by default.')
param sqlAadAdminLogin string

module storage 'storage.bicep' = {
  name: 'storage'
  params: {
    location: location
  }
}

module sql 'sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    sqlAadAdminLogin: sqlAadAdminLogin
  }
}

module appService 'appService.bicep' = {
  name: 'appService'
  params: {
    location: location
    blobStorageServiceUri: storage.outputs.blobEndpoint
    blobStorageContainerName: storage.outputs.documentsContainerName
    // "Active Directory Default" tells Microsoft.Data.SqlClient to authenticate the same way
    // Azure.Identity's DefaultAzureCredential does: managed identity in Azure, your own
    // "az login" locally. No password anywhere in this connection string.
    sqlServerConnectionString: 'Server=tcp:${sql.outputs.sqlServerFqdn},1433;Database=${sql.outputs.sqlDatabaseName};Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;'
  }
}

@description('Built-in "Storage Blob Data Contributor" role: read, write and delete blobs, but no account-level management (keys, network rules, etc).')
var storageBlobDataContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

module storageAccess 'roleAssignment.bicep' = {
  name: 'storageAccess'
  params: {
    storageAccountName: storage.outputs.storageAccountName
    principalId: appService.outputs.principalId
    roleDefinitionId: storageBlobDataContributorRoleId
  }
}

output appServiceName string = appService.outputs.appServiceName
output appServiceHostName string = appService.outputs.defaultHostName
output sqlServerFqdn string = sql.outputs.sqlServerFqdn
output sqlDatabaseName string = sql.outputs.sqlDatabaseName
