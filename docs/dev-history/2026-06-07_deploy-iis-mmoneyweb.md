# Deploy IIS — MMoneyWeb (sessão 2026-06-07)

> Histórico integral da sessão de publicação no Windows Server / IIS.  
> Script automatizado: `scripts/configure-iis-mmoneyweb.ps1`

---

## Ambiente de produção

| Item | Valor |
|------|--------|
| Servidor | `EC2AMAZ-AQ2KOSQ` (Windows Server, IIS) |
| Site IIS | `mmoneyweb.com` (id: **4**, state: Started) |
| Caminho físico | `C:\inetpub\vhosts\mmoneyweb.com` |
| Application Pool | `mmoneyweb.com` — **Sem código gerenciado** (`MgdVersion:` vazio) |
| Binding HTTP inicial | `*:80:www.mmoneyweb.com` (faltava `mmoneyweb.com` sem www) |
| SQL Server | `98.90.225.8,1433` / database `mmoneyweb` |
| Repositório Git | https://github.com/marciocruz-hotmail/MMoneyWeb.git |

---

## Diferença crítica: .NET Framework 4.7 vs ASP.NET Core 10

Os outros sites no mesmo IIS usam pools **CLR v4.0** (.NET Framework). O MMoneyWeb **não pode** compartilhar esse pool.

| | Apps legados (.NET 4.7) | MMoneyWeb |
|--|-------------------------|-----------|
| Pool CLR | v4.0 | **No Managed Code** |
| Módulo IIS | ASP.NET clássico | **AspNetCoreModuleV2** |
| Pré-requisito | .NET Framework | **.NET 10 Hosting Bundle** (não basta SDK) |

Verificar Hosting Bundle:

```powershell
Test-Path "C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
dotnet --list-runtimes   # deve listar Microsoft.AspNetCore.App 10.x
```

---

## Diagnóstico realizado

### App funciona fora do IIS

```powershell
cd C:\inetpub\vhosts\mmoneyweb.com
$env:ASPNETCORE_ENVIRONMENT = "Production"
& "C:\Program Files\dotnet\dotnet.exe" .\MMoneyWeb.Web.dll --urls "http://127.0.0.1:5080"
# Outro terminal:
Invoke-WebRequest http://127.0.0.1:5080 -UseBasicParsing  # → StatusCode 200 (login)
```

Conclusão: código e connection strings OK; falha é **integração IIS**.

### Sintomas no IIS

| Sintoma | Causa provável |
|---------|----------------|
| Página padrão IIS “Welcome” | Binding/caminho errado ou site Default Web Site |
| HTTP 503 | Pool parado ou CLR v4.0 |
| HTTP 500 genérico | Pool/permissões/`dotnet` no PATH/Data Protection |
| Sem `stdout_*.log` | IIS não inicia processo ou sem permissão em `logs` |

### Binding hostname

`appcmd list site "mmoneyweb.com"` mostrou apenas `www.mmoneyweb.com`. Acesso a `mmoneyweb.com` (sem www) pode cair em outro site.

Adicionar binding:

```powershell
C:\Windows\System32\inetsrv\appcmd set site "mmoneyweb.com" /+bindings.[protocol='http',bindingInformation='*:80:mmoneyweb.com']
```

---

## Arquivos e configuração no servidor

### Obrigatórios na pasta de publicação

- `MMoneyWeb.Web.dll`
- `web.config` (gerado/customizado)
- `appsettings.json`
- **`appsettings.Production.json`** (connection strings reais; gitignored)

### `web.config` recomendado (resumo)

- `processPath="C:\Program Files\dotnet\dotnet.exe"` (caminho completo; `dotnet` no PATH não basta para o pool)
- `hostingModel="inprocess"` (após Hosting Bundle instalado)
- `stdoutLogEnabled="true"` + pasta `logs` com permissão para `IIS AppPool\mmoneyweb.com`
- `ASPNETCORE_ENVIRONMENT=Production`
- `webSocket enabled="true"` (Blazor Server)

### Pastas auxiliares

- `logs\` — stdout do módulo ASP.NET Core
- `keys\` — Data Protection (Identity/cookies no IIS)

---

## Alterações no código (repositório)

| Arquivo | Alteração |
|---------|-----------|
| `Program.cs` | `App:RequireHttps` opcional; Data Protection em pasta `keys/` |
| `web.config` | IIS: inprocess, dotnet full path, stdout, module debug |
| `MMoneyWeb.Web.csproj` | Não publicar `Development.json` / `.example` |
| `scripts/configure-iis-mmoneyweb.ps1` | Configuração automatizada IIS (pool, vdir, bindings, permissões, web.config) |
| `scripts/publish-iis.ps1` | Publicação Release para pasta IIS |
| `scripts/iis-diagnostico.ps1` | Diagnóstico rápido |
| `appsettings.json` | `App.RequireHttps: false` (até certificado HTTPS) |

### Script `configure-iis-mmoneyweb.ps1` — notas técnicas

- Compatível com **Windows PowerShell 5.1** (servidor EC2)
- **Não usar** `$Args` como parâmetro de função (variável reservada)
- Caminho físico via: `appcmd set vdir /vdir.name:mmoneyweb.com/ /physicalPath:...`
- CLR vazio: `/managedRuntimeVersion:""`
- Executar: `powershell -ExecutionPolicy Bypass -File .\configure-iis-mmoneyweb.ps1`

---

## Comandos úteis (servidor)

```powershell
# Site e pool
C:\Windows\System32\inetsrv\appcmd list site "mmoneyweb.com"
C:\Windows\System32\inetsrv\appcmd list apppool "mmoneyweb.com"
C:\Windows\System32\inetsrv\appcmd list app "mmoneyweb.com/"

# Log IIS (site id 4)
Get-Content C:\inetpub\logs\LogFiles\W3SVC4\u_ex*.log -Tail 5

# Log app
Get-ChildItem C:\inetpub\vhosts\mmoneyweb.com\logs\stdout*.log | Sort-Object LastWriteTime -Descending | Select -First 1 | Get-Content -Tail 40
```

---

## Fluxo de deploy recomendado

1. Na máquina de build: `dotnet publish src\MMoneyWeb.Web\MMoneyWeb.Web.csproj -c Release -o .\publish\iis`
2. Copiar conteúdo para `C:\inetpub\vhosts\mmoneyweb.com` (manter `appsettings.Production.json` no servidor)
3. No servidor: `powershell -ExecutionPolicy Bypass -File .\configure-iis-mmoneyweb.ps1`
4. Testar: `http://www.mmoneyweb.com` e `http://mmoneyweb.com`
5. Quando HTTPS estiver pronto (win-acme): binding 443 + `App.RequireHttps: true`

---

## Estado ao fim da sessão

- Hosting Bundle .NET 10: **instalado**
- Pool `mmoneyweb.com`: **No Managed Code**, Load User Profile
- App manual (Kestrel :5080): **HTTP 200**
- IIS via script: última correção aplicada (`set vdir` para physicalPath); **aguardar reexecução do script e confirmação HTTP 200 no navegador**
- Pendente: republicar DLL com `RequireHttps` + Data Protection; validar `appsettings.Production.json` no servidor
