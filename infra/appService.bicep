@description('Azure region for the resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Name of the App Service plan.')
param appServicePlanName string = 'plan-fp-${uniqueString(resourceGroup().id)}'

@description('Name of the App Service (web app). Must be globally unique - it becomes <name>.azurewebsites.net.')
param appServiceName string = 'app-fp-${uniqueString(resourceGroup().id)}'

@description('App Service plan pricing tier. F1 (Free) costs nothing but has no Always On and pauses after 20 minutes without traffic (slow first request). Use at least B1 for a smoother demo.')
param skuName string = 'F1'

@description('Blob storage endpoint for the Documents feature, e.g. storage.bicep\'s blobEndpoint output. Left empty, document upload/download fails until configured.')
param blobStorageServiceUri string = ''

@description('Name of the blob container for documents.')
param blobStorageContainerName string = 'documents'

@description('EF Core provider to use: "Sqlite" or "SqlServer".')
param databaseProvider string = 'SqlServer'

@description('SQL Server connection string using Azure AD authentication (no password in it). Left empty, the app falls back to its bundled SQLite file, which is wiped on every deployment.')
param sqlServerConnectionString string = ''

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  kind: 'linux'
  sku: {
    name: skuName
  }
  properties: {
    reserved: true // required for Linux plans
  }
}

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    // Gives the app its own identity in Entra ID, managed by Azure (no credentials to store or rotate).
    // Used below instead of secrets to reach Blob Storage and SQL Database.
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true // the app's auth cookie is Secure-only, so plain HTTP must be rejected, not just redirected
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: skuName != 'F1' // the Free tier doesn't support Always On
      ftpsState: 'Disabled'
      // "__" becomes ":" in ASP.NET Core configuration, so these map to appsettings.json's
      // BlobStorage:ServiceUri, Database:Provider etc. Environment variables like these take
      // precedence over appsettings.json, so nothing needs to change in the committed file.
      appSettings: [
        { name: 'BlobStorage__ServiceUri', value: blobStorageServiceUri }
        { name: 'BlobStorage__ContainerName', value: blobStorageContainerName }
        { name: 'Database__Provider', value: databaseProvider }
        { name: 'ConnectionStrings__SqlServerConnection', value: sqlServerConnectionString }
      ]
    }
  }
}

output appServiceName string = appService.name
output principalId string = appService.identity.principalId
output defaultHostName string = appService.properties.defaultHostName
