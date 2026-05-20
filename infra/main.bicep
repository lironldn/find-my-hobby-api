targetScope = 'subscription'

@description('Azure region for all resources.')
param location string = 'uksouth'

@description('Resource group name.')
param resourceGroupName string = 'find-my-hobby-rg'

@description('AKS cluster name.')
param aksClusterName string = 'find-my-hobby-aks'

@description('Azure Container Registry name. Must be globally unique, letters and numbers only.')
param acrName string

@description('AKS DNS prefix.')
param aksDnsPrefix string = 'find-my-hobby'

@description('AKS Kubernetes version. Leave empty to use Azure default.')
param kubernetesVersion string = ''

@description('AKS system node VM size.')
param nodeVmSize string = 'Standard_D2pls_v6'

@description('AKS system node count.')
@minValue(1)
@maxValue(3)
param nodeCount int = 1

@description('AKS system node pool name. For an existing AKS cluster, this must match the existing system node pool name.')
param systemNodePoolName string = 'nodepool1'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
}

module aksAcr 'aks-acr.bicep' = {
  name: 'aks-acr-deployment'
  scope: resourceGroup
  params: {
    location: location
    aksClusterName: aksClusterName
    acrName: acrName
    aksDnsPrefix: aksDnsPrefix
    kubernetesVersion: kubernetesVersion
    nodeVmSize: nodeVmSize
    nodeCount: nodeCount
    systemNodePoolName: systemNodePoolName
  }
}

output resourceGroupName string = resourceGroup.name
output acrLoginServer string = aksAcr.outputs.acrLoginServer
output aksClusterName string = aksAcr.outputs.aksClusterName