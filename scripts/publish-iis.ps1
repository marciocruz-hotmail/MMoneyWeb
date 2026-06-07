# Publica o MMoneyWeb para pasta IIS (Release).
# Uso local:  .\scripts\publish-iis.ps1
# No servidor: .\scripts\publish-iis.ps1 -OutputPath C:\inetpub\vhosts\mmoneyweb.com

param(
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\publish\iis")
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\src\MMoneyWeb.Web\MMoneyWeb.Web.csproj"
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

Write-Host "Publicando para: $OutputPath"

dotnet publish $project -c Release -o $OutputPath

$dll = Join-Path $OutputPath "MMoneyWeb.Web.dll"
$webConfig = Join-Path $OutputPath "web.config"

if (-not (Test-Path $dll)) { throw "Falha: MMoneyWeb.Web.dll nao foi gerado." }
if (-not (Test-Path $webConfig)) { throw "Falha: web.config nao foi gerado." }

$logs = Join-Path $OutputPath "logs"
New-Item -ItemType Directory -Force -Path $logs | Out-Null

$productionSettings = Join-Path $OutputPath "appsettings.Production.json"
if (-not (Test-Path $productionSettings)) {
    Write-Warning "Crie appsettings.Production.json em $OutputPath com as connection strings de producao."
}

Write-Host "OK: publish concluido."
Write-Host "  MMoneyWeb.Web.dll"
Write-Host "  web.config"
Write-Host "Proximo passo: apontar o site IIS mmoneyweb.com para $OutputPath"
