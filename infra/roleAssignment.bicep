// A role assignment on an "existing" resource whose name comes from another module's output
// must live in its own module - Bicep can't resolve the assignment's name/scope for that case
// directly in the calling file (error BCP120).

@description('Name of the existing storage account to grant access on.')
param storageAccountName string

@description('Object id of the principal (e.g. a managed identity) to grant the role to.')
param principalId string

@description('Object id (GUID) of the built-in role definition to assign, e.g. Storage Blob Data Contributor.')
param roleDefinitionId string

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  // A deterministic name (not a random guid) so re-running this deployment updates the same
  // assignment instead of failing on a duplicate.
  name: guid(storageAccount.id, principalId, roleDefinitionId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
