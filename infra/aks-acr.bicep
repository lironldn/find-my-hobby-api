targetScope = 'resourceGroup'

@description('Azure region for all resources.')
param location string

@description('AKS cluster name.')
param aksClusterName string

@description('Azure Container Registry name. Must be globally unique, letters and numbers only.')
param acrName string

@description('AKS DNS prefix.')
param aksDnsPrefix string

@description('AKS Kubernetes version. Leave empty to use Azure default.')
param kubernetesVersion string

@description('AKS system node VM size.')
param systemNodeVmSize string

@description('AKS system node count.')
param systemNodeCount int

@description('AKS system node pool name. For an existing AKS cluster, this must match the existing system node pool name.')
param systemNodePoolName string = 'nodepool1'

@description('AKS user node VM size for application workloads.')
param userNodeVmSize string

@description('AKS user node count.')
param userNodeCount int

@description('AKS user node pool name.')
param userNodePoolName string = 'workloadpool'

@description('Label value used to steer application pods to the user node pool.')
param workloadNodeLabelValue string = 'apps'

var acrPullRoleDefinitionId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  '7f951dda-4ed3-4680-a7ca-43fe172d538d'
)

resource acr 'Microsoft.ContainerRegistry/registries@2023-11-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource aks 'Microsoft.ContainerService/managedClusters@2024-05-01' = {
  name: aksClusterName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: aksDnsPrefix

    kubernetesVersion: empty(kubernetesVersion) ? null : kubernetesVersion

    agentPoolProfiles: [
      {
        name: systemNodePoolName
        count: systemNodeCount
        vmSize: systemNodeVmSize
        osType: 'Linux'
        osSKU: 'Ubuntu'
        mode: 'System'
        type: 'VirtualMachineScaleSets'
        enableAutoScaling: false
        nodeTaints: [
          'CriticalAddonsOnly=true:NoSchedule'
        ]
      }
    ]

    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      loadBalancerSku: 'standard'
      outboundType: 'loadBalancer'
    }

    apiServerAccessProfile: {
      enablePrivateCluster: false
    }
  }
}

resource userAgentPool 'Microsoft.ContainerService/managedClusters/agentPools@2024-05-01' = {
  parent: aks
  name: userNodePoolName
  properties: {
    count: userNodeCount
    vmSize: userNodeVmSize
    osType: 'Linux'
    osSKU: 'Ubuntu'
    mode: 'User'
    type: 'VirtualMachineScaleSets'
    enableAutoScaling: false
    nodeLabels: {
      workload: workloadNodeLabelValue
    }
  }
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(acr.id, aks.id, acrPullRoleDefinitionId)
  scope: acr
  properties: {
    roleDefinitionId: acrPullRoleDefinitionId
    principalId: aks.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output acrLoginServer string = acr.properties.loginServer
output aksClusterName string = aks.name
output aksKubeletObjectId string = aks.identity.principalId
