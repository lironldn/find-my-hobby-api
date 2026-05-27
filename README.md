# find-my-hobby-api

`find-my-hobby-api` is an API for helping people discover hobbies and find real UK classes or courses for a specific interest, postcode, and travel radius.

## Project Links

- Jira board: [FindMyHobby board](https://lironschur.atlassian.net/jira/software/projects/FMH/boards/1)

## Jira Integration

Use a dedicated Jira service account for automation. The API token belongs to that account, and Jira still enforces the account's normal product and space permissions.

### 1. Create or choose a Jira service account

Use a non-personal Atlassian account for the FindMyHobby space.

### 2. Grant the account access in Jira

- App access: `Jira -> User`
- Space role: `Member`
- Use `Viewer` only if the account is read-only

Do not grant `Jira Administration` or `User Access Admin` unless the account needs to administer Jira itself.

### 3. Create a scoped API token

Create a `JIRA_API_TOKEN` for the service account. The helper sends it as a bearer token to `api.atlassian.com`.

### 4. Set local environment variables

Configure these values in your local environment or secret store:

```bash
JIRA_BASE_URL=https://api.atlassian.com
JIRA_CLOUD_ID=0a074c07-2971-4259-a9da-d983675e20d1
JIRA_API_TOKEN=...
JIRA_PROJECT_KEY=FMH
```

### 5. Use the Jira REST API

- Read a ticket: `GET /rest/api/3/issue/{issueKey}`
- Update labels or description: `PUT /rest/api/3/issue/{issueKey}`

### 6. Create issues with this template

Copy this into the Jira story description when creating a new issue:

```text
Goal:

Context:

Acceptance criteria:
- 

Implementation notes:

Dependencies / blockers:

Test notes:

Definition of done:
```

### 7. Use the local Jira helper

The repository includes a small CLI helper in `JiraTool` for common Jira operations.
It reads `JIRA_BASE_URL`, `JIRA_CLOUD_ID`, and `JIRA_API_TOKEN` from a local `.env` file when present, and otherwise falls back to the process environment.

Examples:

```bash
JIRA_BASE_URL=https://api.atlassian.com \
JIRA_CLOUD_ID=0a074c07-2971-4259-a9da-d983675e20d1 \
JIRA_API_TOKEN=... \
dotnet run --project JiraTool/JiraTool.csproj show FMH-5

JIRA_BASE_URL=https://api.atlassian.com \
JIRA_CLOUD_ID=0a074c07-2971-4259-a9da-d983675e20d1 \
JIRA_API_TOKEN=... \
dotnet run --project JiraTool/JiraTool.csproj ready FMH-5
```

The `ready` command reads the available transitions for the issue and moves it to the `READY` status when that transition exists.

## Project Objectives

The project is designed to:

- suggest hobby ideas quickly
- search for currently available UK courses and classes related to a hobby
- return structured results that can be consumed by a UI or another service
- run in containers and deploy to AKS

## Features

### `GET /hobby`

Returns 5 random hobby suggestions.

Example:

```bash
curl http://localhost:8080/hobby
```

Example response:

```json
[
  { "name": "Pottery" },
  { "name": "Cooking" },
  { "name": "Karaoke" },
  { "name": "Hiking" },
  { "name": "Drawing" }
]
```

### `POST /courses/search`

Searches for real UK courses, classes, or workshops related to a hobby and constrained by postcode and maximum distance.

Request body:

```json
{
  "hobbyDescription": "beginner pottery classes",
  "postcode": "SW1A 1AA",
  "maximumDistanceMiles": 10
}
```

Example call:

```bash
curl -X POST http://localhost:8080/courses/search \
  -H "Content-Type: application/json" \
  -d '{
    "hobbyDescription": "beginner pottery classes",
    "postcode": "SW1A 1AA",
    "maximumDistanceMiles": 10
  }'
```

Response shape:

```json
{
  "query": {
    "hobbyDescription": "beginner pottery classes",
    "postcode": "SW1A 1AA",
    "maximumDistanceMiles": 10
  },
  "results": [
    {
      "title": "Beginner Course",
      "providerName": "Example Studio",
      "description": "Introductory pottery course for beginners",
      "address": "Example address",
      "postcode": "SW1A 1AA",
      "estimatedDistanceMiles": 1.2,
      "distanceIsEstimated": true,
      "price": "£90 per person",
      "schedule": "Thursday evenings",
      "bookingUrl": "https://example.com/book",
      "sourceUrl": "https://example.com"
    }
  ],
  "notes": "..."
}
```

Validation rules:

- `hobbyDescription` is required
- `postcode` is required
- `maximumDistanceMiles` must be greater than 0
- `maximumDistanceMiles` must be 100 or less

If the OpenAI API key is not configured, the endpoint returns a server error.

### `GET /health`

Returns a simple health check response.

Example:

```bash
curl http://localhost:8080/health
```

Example response:

```json
{ "status": "Healthy" }
```

## Local configuration

The app reads `FindMyHobbyApi/appsettings.Local.json` if it exists. Use it for local-only settings such as:

```json
{
  "OPENAI_API_KEY": "sk-local-dummy-value"
}
```

## Running locally with Docker

The `Dockerfile` builds a production image that listens on port `8080`.

Example:

```bash
docker build -t find-my-hobby-api .
docker run --rm -p 8080:8080 find-my-hobby-api
```

## Cloud deployment

The repository includes three GitHub Actions workflows:

### `deploy-to-aks.yml`

Builds and publishes the API and website container images, then deploys the API to AKS, runs the acceptance tests, and deploys the website after the backend stage completes.

Flow:

1. Build the Docker image from `Dockerfile`.
2. Push the image to Azure Container Registry.
3. Wait for approval on the `aks` GitHub environment.
4. Pull AKS credentials.
5. Apply `k8s/deployment.yaml` and `k8s/service.yaml`.
6. Update the deployment image to the SHA-tagged image.
7. Wait for rollout and for the service external IP.
8. Run the acceptance tests against the deployed service.
9. Build and publish the website image.
10. Deploy the website to AKS after the backend stage has completed successfully.

### `deploy-infra.yml`

Validates and deploys the Azure infrastructure from `infra/main.bicep` and `infra/main.parameters.json`.

It creates or updates:

- the resource group
- Azure Container Registry
- AKS
- the ACR pull role assignment for AKS

### `schedule-aks.yml`

Starts, stops, or checks the AKS cluster.

- scheduled runs stop the cluster outside working hours
- manual runs can `start`, `stop`, or show `status`

## Frontend

The React + TypeScript frontend lives in [FindMyHobbyWeb](FindMyHobbyWeb/README.md).

It renders a hobby search form, calls the API, and displays the returned courses in a table.
The container image is intended to run in the same AKS cluster as the API (to save costs).

When AKS is up and running, the website is available at:

http://findmyhobby.schur-thing.com

Public access:

- `findmyhobby.schur-thing.com` is managed in Squarespace and points at the AKS load balancer.
- The Kubernetes `LoadBalancer` services still provision and wait on their own external IPs during deployment.

## Run Locally

Use two terminals.

### 1. Start the API

```bash
docker compose up --build
```

This starts the API on `http://localhost:5001`.

If you want `POST /courses/search` to work locally, set `OPENAI_API_KEY` in `FindMyHobbyApi/appsettings.Local.json`.

### 2. Start the frontend

```bash
cd FindMyHobbyWeb
npm install
npm run dev
```

Open the Vite URL it prints, usually `http://localhost:5173`.

The frontend proxies `/api/*` to `http://localhost:5001`, so it will talk to the local API without extra CORS setup.

## Infrastructure Deployment

Infrastructure is deployed with Bicep through GitHub Actions, not by manually creating resources in the portal.

The main template is `infra/main.bicep`, which delegates to `infra/aks-acr.bicep`.

Deployment steps:

1. GitHub Actions logs in to Azure using OIDC.
2. The template is validated with `az deployment sub validate`.
3. The subscription-scoped deployment creates or updates the resource group.
4. The resource group-scoped module creates ACR and AKS.
5. AKS is granted `AcrPull` on the registry.
6. The deployment outputs the resource group, AKS cluster name, and ACR login server.

The current parameter file contains placeholders for the real environment values. Replace these before deployment if you are not using the defaults:

```json
{
  "parameters": {
    "location": { "value": "uksouth" },
    "resourceGroupName": { "value": "<RESOURCE_GROUP_NAME>" },
    "aksClusterName": { "value": "<AKS_CLUSTER_NAME>" },
    "acrName": { "value": "<ACR_NAME>" },
    "aksDnsPrefix": { "value": "<AKS_DNS_PREFIX>" },
    "kubernetesVersion": { "value": "" },
    "nodeVmSize": { "value": "Standard_D2pls_v6" },
    "nodeCount": { "value": 1 },
    "systemNodePoolName": { "value": "nodepool1" }
  }
}
```

## Acceptance Tests

The acceptance test project is `FindMyHobbyApi.AcceptanceTests`.

It currently verifies:

- the API responds to `GET /health`
- `GET /hobby` returns `200`
- `GET /hobby` returns 5 hobby suggestions
- each hobby has a non-empty `name`

The tests read the API base URL from `FIND_MY_HOBBY_API_BASE_URL`. If that environment variable is not set, they default to `http://localhost:5001`.

The workflow runs the tests after deploying to AKS so they exercise the live service rather than a local container.

## AKS startup script

Start the cluster:

```bash
az aks start \
  --resource-group find-my-hobby-rg \
  --name find-my-hobby-aks
```

Stop the cluster:

```bash
az aks stop \
  --resource-group find-my-hobby-rg \
  --name find-my-hobby-aks
```

## Manual Setup

The repository expects a small amount of one-time Azure and GitHub setup.

### Azure

1. Create or identify an Azure app registration or managed identity for GitHub Actions.
2. Create federated identity credentials for the GitHub repository:
   - branch build/publish credential:
     - subject: `repo:<GITHUB_OWNER>/<GITHUB_REPO>:ref:refs/heads/main`
   - deploy credential for the `aks` environment:
     - subject: `repo:<GITHUB_OWNER>/<GITHUB_REPO>:environment:aks`
   - issuer: `https://token.actions.githubusercontent.com`
   - audience: `api://AzureADTokenExchange`
3. Make sure the identity has permission to deploy the infrastructure and access AKS.
4. Run the infrastructure workflow once so the resource group, ACR, and AKS exist.

Example Azure CLI commands:

```bash
az ad app federated-credential create \
  --id <APP_ID> \
  --parameters '{
    "name": "github-main-<REPO_NAME>",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<GITHUB_OWNER>/<GITHUB_REPO>:ref:refs/heads/main",
    "description": "GitHub Actions OIDC for main branch",
    "audiences": [
      "api://AzureADTokenExchange"
    ]
  }'

az ad app federated-credential create \
  --id <APP_ID> \
  --parameters '{
    "name": "github-aks-<REPO_NAME>",
    "issuer": "https://token.actions.githubusercontent.com",
    "subject": "repo:<GITHUB_OWNER>/<GITHUB_REPO>:environment:aks",
    "description": "GitHub Actions OIDC for aks environment",
    "audiences": [
      "api://AzureADTokenExchange"
    ]
  }'
```

### GitHub

1. Add repository secrets:
   - `AZURE_CLIENT_ID`
   - `AZURE_TENANT_ID`
   - `AZURE_SUBSCRIPTION_ID`
   - `ACR_NAME`
   - `AKS_RESOURCE_GROUP`
   - `AKS_CLUSTER_NAME`
2. Create the GitHub environment `aks`.
3. Add required reviewers to the `aks` environment if you want the deployment job to pause for approval.
4. If needed, set up any additional environment secrets used by your deployment process.
