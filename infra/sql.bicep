@description('Azure region for the resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Name of the logical SQL server. Must be globally unique.')
param sqlServerName string = 'sql-fp-${uniqueString(resourceGroup().id)}'

@description('Name of the database.')
param sqlDatabaseName string = 'CompanyPortal'

@description('Display name for the Azure AD admin of the SQL server, e.g. your work email. Just a label shown in the Azure Portal / SSMS - not a password or a login you type anywhere.')
param sqlAadAdminLogin string

@description('Object id of the Azure AD admin. Defaults to whoever runs this deployment.')
param sqlAadAdminObjectId string = deployer().objectId

@description('Azure AD tenant that the SQL server trusts for authentication.')
param sqlAadAdminTenantId string = subscription().tenantId

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    // No SQL login/password exists on this server at all - only Azure AD (Entra ID) identities
    // can authenticate. Same passwordless approach already used for Blob Storage.
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: sqlAadAdminLogin
      sid: sqlAadAdminObjectId
      tenantId: sqlAadAdminTenantId
      azureADOnlyAuthentication: true
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic' // cheapest DTU tier (~$5/month) - fine for an exercise, not for production load
    tier: 'Basic'
  }
  properties: {
    maxSizeBytes: 2147483648 // 2 GB: the Basic tier's limit
  }
}

// Opens the network path from Azure services (including this project's App Service) to the server.
// A valid Azure AD token is still required to actually log in - this rule alone grants no access.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output sqlServerName string = sqlServer.name
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
