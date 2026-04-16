param(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

    [Parameter(Mandatory = $true)]
    [string]$Location,

    [Parameter(Mandatory = $true)]
    [string]$EnvironmentName,

    [Parameter(Mandatory = $true)]
    [string]$AppName,

    [Parameter(Mandatory = $true)]
    [string]$Image,

    [switch]$EnableSwagger
)

$swaggerValue = if ($EnableSwagger) { "true" } else { "false" }

az group create -n $ResourceGroup -l $Location
az containerapp env create -g $ResourceGroup -n $EnvironmentName -l $Location

az containerapp create `
  -g $ResourceGroup `
  -n $AppName `
  --environment $EnvironmentName `
  --image $Image `
  --target-port 8080 `
  --ingress external `
  --min-replicas 1 `
  --env-vars Swagger__Enabled=$swaggerValue ASPNETCORE_URLS=http://+:8080

az containerapp logs show -g $ResourceGroup -n $AppName --follow
