# TrainingService

## Run `Training.Api` in Docker locally

From repo root (`C:\Repos\TrainingService\TrainingService`):

1. Build image:

```powershell
docker build -f Training.Api/Dockerfile -t training-api:local .
```

2. Run container:

```powershell
docker run --rm -d -p 8080:8080 --name training-api-local training-api:local
```

3. Verify endpoints:

- Health: `http://localhost:8080/health`
- Swagger: `http://localhost:8080/swagger` (only if `Swagger__Enabled=true` is provided)

4. Optional: run with Swagger enabled:

```powershell
docker run --rm -d -p 8080:8080 --name training-api-local -e Swagger__Enabled=true training-api:local
```

5. Stop container:

```powershell
docker stop training-api-local
```

## Azure Container Apps quick deploy

Prerequisites:
- Azure CLI installed
- Logged in: `az login`
- Container image pushed to a registry (e.g., ACR)

Example commands:

```powershell
$RG="rg-training-dev"
$LOC="eastus"
$ENV="cae-training-dev"
$APP="training-api"
$IMAGE="<your-registry>/training-api:latest"

az group create -n $RG -l $LOC
az containerapp env create -g $RG -n $ENV -l $LOC

az containerapp create `
  -g $RG `
  -n $APP `
  --environment $ENV `
  --image $IMAGE `
  --target-port 8080 `
  --ingress external `
  --min-replicas 1 `
  --env-vars Swagger__Enabled=true ASPNETCORE_URLS=http://+:8080
```

After deploy, verify:
- `/health` is reachable
- `/swagger` is reachable
- Logs are visible:

```powershell
az containerapp logs show -g $RG -n $APP --follow
```

## Optional deployment script

You can run the parameterized script instead of copying commands:

```powershell
pwsh ./scripts/Deploy-TrainingApiToAca.ps1 \
  -ResourceGroup rg-training-dev \
  -Location eastus \
  -EnvironmentName cae-training-dev \
  -AppName training-api \
  -Image <your-registry>/training-api:latest \
  -EnableSwagger
```
