targetScope = 'subscription'

@description('Azure region for all resources.')
param location string = 'northeurope'

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
param systemNodeVmSize string = 'Standard_EC4as_v5'

@description('AKS system node count.')
@minValue(1)
@maxValue(3)
param systemNodeCount int = 1

@description('AKS system node pool name. For an existing AKS cluster, this must match the existing system node pool name.')
param systemNodePoolName string = 'nodepool1'

@description('AKS user node VM size for application workloads.')
param userNodeVmSize string = 'Standard_B2ls_v2'

@description('AKS user node count.')
@minValue(0)
@maxValue(3)
param userNodeCount int = 1

@description('AKS user node pool name.')
param userNodePoolName string = 'workloadpool'

@description('Label value used to steer application pods to the user node pool.')
param workloadNodeLabelValue string = 'apps'

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
    systemNodeVmSize: systemNodeVmSize
    systemNodeCount: systemNodeCount
    systemNodePoolName: systemNodePoolName
    userNodeVmSize: userNodeVmSize
    userNodeCount: userNodeCount
    userNodePoolName: userNodePoolName
    workloadNodeLabelValue: workloadNodeLabelValue
  }
}

output resourceGroupName string = resourceGroup.name
output acrLoginServer string = aksAcr.outputs.acrLoginServer
output aksClusterName string = aksAcr.outputs.aksClusterName
