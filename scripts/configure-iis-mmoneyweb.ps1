#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Configura o site MMoneyWeb no IIS (Windows Server).

.EXAMPLE
    .\configure-iis-mmoneyweb.ps1
#>
[CmdletBinding()]
param(
    [string]$SiteName = "mmoneyweb.com",
    [string]$PoolName = "mmoneyweb.com",
    [string]$SitePath = "C:\inetpub\vhosts\mmoneyweb.com",
    [switch]$SkipIisReset
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$AppCmd = "$env:windir\System32\inetsrv\appcmd.exe"
$DotNetExe = "${env:ProgramFiles}\dotnet\dotnet.exe"
$AspNetCoreModule = "${env:ProgramFiles}\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"

function Write-Step([string]$Message) {
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Test-CommandPath([string]$Path, [string]$Label) {
    if (-not (Test-Path $Path)) {
        throw "$Label nao encontrado: $Path"
    }
    Write-Host "  OK: $Label"
}

function Invoke-AppCmd {
    param(
        [string[]]$CommandArgs
    )

    if (-not $CommandArgs -or $CommandArgs.Count -eq 0) {
        throw "Invoke-AppCmd: CommandArgs vazio."
    }

    $output = & $AppCmd @CommandArgs 2>&1
    $text = ($output | Out-String).Trim()
    if ($LASTEXITCODE -ne 0) {
        throw "appcmd falhou: appcmd $($CommandArgs -join ' ')`n$text"
    }
    return $output
}

Write-Step "Verificando pre-requisitos"
Test-CommandPath $AppCmd "appcmd.exe"
Test-CommandPath $DotNetExe "dotnet.exe"
Test-CommandPath $AspNetCoreModule "ASP.NET Core Module V2 (instale o .NET Hosting Bundle)"
Test-CommandPath (Join-Path $SitePath "MMoneyWeb.Web.dll") "MMoneyWeb.Web.dll"

$runtime = (& $DotNetExe --list-runtimes 2>&1 | Out-String)
if ($runtime -notmatch "Microsoft\.AspNetCore\.App 10\.") {
    Write-Warning "Runtime Microsoft.AspNetCore.App 10.x nao encontrado. Instale o .NET 10 Hosting Bundle."
} else {
    Write-Host "  OK: ASP.NET Core 10 runtime"
}

Write-Step "Garantindo Application Pool '$PoolName'"
$existingPools = (& $AppCmd list apppool 2>&1 | Out-String)
if ($existingPools -notmatch "APPPOOL `"$PoolName`"") {
    Invoke-AppCmd -CommandArgs @("add", "apppool", "/name:$PoolName") | Out-Null
    Write-Host "  Pool criado: $PoolName"
}

Invoke-AppCmd -CommandArgs @("set", "apppool", "/apppool.name:$PoolName", '/managedRuntimeVersion:""') | Out-Null
Invoke-AppCmd -CommandArgs @("set", "apppool", "/apppool.name:$PoolName", "/managedPipelineMode:Integrated") | Out-Null
Invoke-AppCmd -CommandArgs @("set", "apppool", "/apppool.name:$PoolName", "/processModel.loadUserProfile:true") | Out-Null
Invoke-AppCmd -CommandArgs @("set", "apppool", "/apppool.name:$PoolName", "/enable32BitAppOnWin64:false") | Out-Null
Invoke-AppCmd -CommandArgs @("start", "apppool", "/apppool.name:$PoolName") | Out-Null
Write-Host "  OK: Pool sem codigo gerenciado, Load User Profile=true"

Write-Step "Verificando site '$SiteName'"
$sites = (& $AppCmd list site 2>&1 | Out-String)
if ($sites -notmatch "SITE `"$SiteName`"") {
    throw "Site IIS '$SiteName' nao encontrado. Crie o site no IIS apontando para $SitePath"
}

Invoke-AppCmd -CommandArgs @("set", "vdir", "/vdir.name:${SiteName}/", "/physicalPath:$SitePath") | Out-Null
Invoke-AppCmd -CommandArgs @("set", "app", "/app.name:${SiteName}/", "/applicationPool:$PoolName") | Out-Null
Write-Host "  OK: Caminho fisico e pool associados"

Write-Step "Configurando bindings HTTP (com e sem www)"
$bindings = (& $AppCmd list site "/site.name:$SiteName" /config 2>&1 | Out-String)
if ($bindings -notmatch '\*:80:mmoneyweb\.com') {
    Invoke-AppCmd -CommandArgs @("set", "site", "/site.name:$SiteName", "/+bindings.[protocol='http',bindingInformation='*:80:mmoneyweb.com']") | Out-Null
    Write-Host "  Binding adicionado: mmoneyweb.com"
}
if ($bindings -notmatch 'www\.mmoneyweb\.com') {
    Invoke-AppCmd -CommandArgs @("set", "site", "/site.name:$SiteName", "/+bindings.[protocol='http',bindingInformation='*:80:www.mmoneyweb.com']") | Out-Null
    Write-Host "  Binding adicionado: www.mmoneyweb.com"
}
Write-Host "  OK: Bindings HTTP"

Write-Step "Criando pastas logs e keys"
$logsPath = Join-Path $SitePath "logs"
$keysPath = Join-Path $SitePath "keys"
New-Item -ItemType Directory -Force -Path $logsPath | Out-Null
New-Item -ItemType Directory -Force -Path $keysPath | Out-Null
Write-Host "  OK: $logsPath"
Write-Host "  OK: $keysPath"

Write-Step "Aplicando permissoes NTFS"
$poolIdentity = "IIS AppPool\$PoolName"
icacls $SitePath /grant "${poolIdentity}:(OI)(CI)M" /T /C | Out-Null
icacls $logsPath /grant "${poolIdentity}:(OI)(CI)F" /T /C | Out-Null
icacls $keysPath /grant "${poolIdentity}:(OI)(CI)F" /T /C | Out-Null
icacls $SitePath /grant "IIS_IUSRS:(OI)(CI)RX" /T /C | Out-Null
Write-Host "  OK: Permissoes para $poolIdentity e IIS_IUSRS"

Write-Step "Gravando web.config"
$webConfigPath = Join-Path $SitePath "web.config"
$webConfigContent = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="$DotNetExe"
                  arguments=".\MMoneyWeb.Web.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile="$logsPath\stdout"
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
          <environmentVariable name="ASPNETCORE_DETAILEDERRORS" value="true" />
          <environmentVariable name="ASPNETCORE_MODULE_DEBUG" value="1" />
        </environmentVariables>
      </aspNetCore>
      <webSocket enabled="true" />
    </system.webServer>
  </location>
</configuration>
"@
Set-Content -Path $webConfigPath -Value $webConfigContent -Encoding UTF8
Write-Host "  OK: $webConfigPath"

Write-Step "Verificando appsettings.Production.json"
$productionSettings = Join-Path $SitePath "appsettings.Production.json"
$developmentSettings = Join-Path $SitePath "appsettings.Development.json"
$baseSettings = Join-Path $SitePath "appsettings.json"

if (-not (Test-Path $productionSettings)) {
    if (Test-Path $developmentSettings) {
        Copy-Item $developmentSettings $productionSettings -Force
        Write-Warning "appsettings.Production.json criado a partir de Development.json. Revise credenciais de producao."
    } else {
        $template = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR,1433;Database=mmoneyweb;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "MMoneyConnection": "Server=SEU_SERVIDOR,1433;Database=mmoneyweb;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "App": {
    "RequireHttps": false
  },
  "AllowedHosts": "*"
}
"@
        Set-Content -Path $productionSettings -Value $template -Encoding UTF8
        Write-Warning "appsettings.Production.json criado com placeholders. EDITE as connection strings antes de usar em producao."
    }
} else {
    Write-Host "  OK: appsettings.Production.json ja existe"
}

if (Test-Path $baseSettings) {
    $json = Get-Content $baseSettings -Raw | ConvertFrom-Json
    if (-not $json.App) {
        $json | Add-Member -NotePropertyName App -NotePropertyValue ([pscustomobject]@{ RequireHttps = $false }) -Force
        $json | ConvertTo-Json -Depth 5 | Set-Content $baseSettings -Encoding UTF8
        Write-Host "  OK: App.RequireHttps=false adicionado em appsettings.json"
    }
}

Write-Step "Habilitando WebSocket Protocol (se disponivel)"
try {
    $ws = (dism /online /get-featureinfo /featurename:IIS-WebSockets 2>&1 | Out-String)
    if ($ws -match "State : Disabled") {
        dism /online /enable-feature /featurename:IIS-WebSockets /all /norestart | Out-Null
        Write-Host "  OK: WebSocket Protocol habilitado"
    } else {
        Write-Host "  OK: WebSocket Protocol ja habilitado ou nao aplicavel"
    }
} catch {
    Write-Warning "Nao foi possivel verificar WebSocket via DISM. Habilite manualmente em Recursos do Windows."
}

Write-Step "Parando processos dotnet manuais (liberar DLL)"
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "  OK: Nenhum dotnet manual em execucao (ou finalizado)"

if (-not $SkipIisReset) {
    Write-Step "Reiniciando IIS"
    iisreset | Out-Null
    Write-Host "  OK: iisreset concluido"
}

Write-Step "Teste local via IIS (Host: www.mmoneyweb.com)"
Start-Sleep -Seconds 2
try {
    $response = Invoke-WebRequest -Uri "http://127.0.0.1" -Headers @{ Host = "www.mmoneyweb.com" } -MaximumRedirection 0 -UseBasicParsing -TimeoutSec 30
    Write-Host "  SUCESSO: HTTP $($response.StatusCode)" -ForegroundColor Green
} catch {
    if ($_.Exception.Response) {
        $code = [int]$_.Exception.Response.StatusCode
        Write-Warning "  Resposta HTTP $code - verifique logs abaixo"
    } else {
        Write-Warning "  Falha no teste: $($_.Exception.Message)"
    }
}

Write-Step "Logs para diagnostico"
$stdoutLogs = Get-ChildItem (Join-Path $logsPath "stdout*.log") -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if ($stdoutLogs) {
    Write-Host "  stdout: $($stdoutLogs.FullName)"
    Get-Content $stdoutLogs.FullName -Tail 15
} else {
    Write-Host "  Nenhum stdout_*.log gerado ainda."
}

$siteListLine = (& $AppCmd list site "/site.name:$SiteName" 2>&1 | Out-String)
$siteId = $null
if ($siteListLine -match 'id:(\d+)') {
    $siteId = $Matches[1]
}
if ($siteId) {
    $w3LogDir = Join-Path $env:SystemDrive "inetpub\logs\LogFiles\W3SVC$siteId"
    $w3Log = Get-ChildItem (Join-Path $w3LogDir "u_ex*.log") -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($w3Log) {
        Write-Host "  IIS log: $($w3Log.FullName)"
        Get-Content $w3Log.FullName -Tail 3
    }
}

$recentEvents = Get-WinEvent -LogName Application -MaxEvents 30 -ErrorAction SilentlyContinue |
    Where-Object { $_.TimeCreated -gt (Get-Date).AddMinutes(-10) -and $_.Message -match "AspNetCore|aspnetcore|MMoneyWeb|IIS AspNetCore" } |
    Select-Object -First 5 TimeCreated, ProviderName, Message
if ($recentEvents) {
    Write-Host "  Eventos Application (ultimos 10 min):"
    $recentEvents | Format-List
}

Write-Host ""
Write-Host "Configuracao concluida." -ForegroundColor Green
Write-Host "Teste no navegador: http://www.mmoneyweb.com e http://mmoneyweb.com"
Write-Host ""
Write-Host "Se ainda falhar:"
Write-Host "  1) Edite $productionSettings com connection strings reais"
Write-Host "  2) Republica a DLL mais recente (dotnet publish) em $SitePath"
Write-Host "  3) Rode novamente este script"
