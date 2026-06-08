# Inicializa repositório Git local e cria o primeiro commit do MMoneyWeb.
# Uso: .\scripts\init-git-repo.ps1
# Depois: git remote add origin <URL> && git push -u origin main

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root

$git = Get-Command git -ErrorAction SilentlyContinue
if (-not $git) {
    $candidates = @(
        "C:\Program Files\Git\cmd\git.exe",
        "C:\Program Files\Git\bin\git.exe"
    )
    foreach ($path in $candidates) {
        if (Test-Path $path) {
            $git = @{ Source = $path }
            break
        }
    }
}

if (-not $git) {
    Write-Host "ERRO: Git nao encontrado." -ForegroundColor Red
    Write-Host "Instale: https://git-scm.com/download/win"
    Write-Host "Depois reexecute: .\scripts\init-git-repo.ps1"
    exit 1
}

function Invoke-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$Args)
    & $git.Source @Args
    if ($LASTEXITCODE -ne 0) { throw "git $($Args -join ' ') falhou (exit $LASTEXITCODE)" }
}

if (Test-Path ".git") {
    Write-Host "Repositorio Git ja existe em $root"
} else {
    Write-Host "Inicializando repositorio Git..."
    Invoke-Git init
    Invoke-Git branch -M main
}

Write-Host "Status antes do commit:"
Invoke-Git status --short

$hasChanges = (Invoke-Git status --porcelain) -ne ""
if (-not $hasChanges) {
    Write-Host "Nada para commitar (working tree limpa)."
    exit 0
}

Invoke-Git add .
Invoke-Git commit -m @"
Preparar deploy Linux com Coolify (Dockerfile, CI, health check).

Inclui containerizacao, proxy reverso, volume /app/keys e guia de deploy automatico via Git.
"@

Write-Host ""
Write-Host "OK: commit inicial criado na branch main." -ForegroundColor Green
Write-Host ""
Write-Host "Proximos passos:"
Write-Host "  1. Criar repo vazio no GitHub/GitLab"
Write-Host "  2. git remote add origin https://github.com/SEU_USUARIO/MMoneyWeb.git"
Write-Host "  3. git push -u origin main"
Write-Host "  4. Seguir docs/deploy-coolify-checklist.md no painel Coolify"
