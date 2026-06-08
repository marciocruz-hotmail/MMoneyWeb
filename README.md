# MMoneyWeb

Aplicativo web pessoal de controle financeiro. Este repositório contém a **fundação técnica** do projeto; funcionalidades de negócio (cadastros, dashboards, regras financeiras) serão implementadas em etapas futuras.

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 10 LTS |
| Linguagem | C# |
| UI | ASP.NET Core Blazor Web App (Interactive Server) |
| Dados | SQL Server + Entity Framework Core 10 |
| Autenticação | ASP.NET Core Identity (contas individuais) |
| Testes | xUnit |
| Hospedagem alvo | Linux + Coolify (Docker) — IIS Windows legado opcional |

## Estrutura de pastas

```text
MMoneyWeb/
├── MMoneyWeb.sln
├── README.md
├── AI-CONTEXT.md          # Contexto fixo para agentes IA
├── CHANGELOG-DEV.md       # Changelog operacional
├── BACKLOG-DEV.md         # Pendências
├── src/
│   └── MMoneyWeb.Web/
│       ├── Components/    # Blazor (páginas, layout, Identity)
│       ├── Data/          # ApplicationDbContext, MMoneyDbContext, Migrations
│       ├── Domain/        # Entidades de domínio (futuro)
│       └── Services/      # Serviços de aplicação (futuro)
├── tests/
│   └── MMoneyWeb.Tests/
└── docs/
    └── dev-history/       # Histórico arquivo
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (LTS)
- SQL Server (local ou instância acessível) — LocalDB também funciona em desenvolvimento se ajustar a connection string
- (Publicação Linux) Servidor com [Coolify](https://coolify.io/) e Docker
- (Publicação Windows legado) IIS + [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)

## Strings de conexão

| Nome | Uso |
|------|-----|
| `DefaultConnection` | ASP.NET Core Identity (`ApplicationDbContext`) |
| `MMoneyConnection` | Entidades financeiras futuras (`MMoneyDbContext`) |

**Desenvolvimento local:** copiar `src/MMoneyWeb.Web/appsettings.Development.example.json` para `appsettings.Development.json` (este último está no `.gitignore`) e preencher servidor, banco, usuário e senha.

Formato recomendado (SQL Server remoto via TCP):

```text
Server=SEU_IP,1433;Database=mmoneyweb;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Use a porta `,1433` para evitar fallback em Named Pipes ao usar `localhost`.

**Migrations do Identity** (somente tabelas `AspNet*` — não altera tabelas de negócio existentes):

```powershell
dotnet ef database update --project src\MMoneyWeb.Web --context ApplicationDbContext
```

**Importante:**

- Em **produção**, credenciais via variável de ambiente, User Secrets ou configuração protegida do IIS.
- Não commitar senhas no repositório (`appsettings.Development.json` está ignorado pelo Git).

## Comandos

### Restaurar pacotes

```powershell
dotnet restore MMoneyWeb.sln
```

### Compilar

```powershell
dotnet build MMoneyWeb.sln -c Release
```

### Executar localmente

```powershell
dotnet run --project src\MMoneyWeb.Web\MMoneyWeb.Web.csproj
```

Na primeira execução em desenvolvimento, o EF Core pode aplicar migrations do Identity automaticamente (página de migrations em modo Development).

### Executar testes

```powershell
dotnet test MMoneyWeb.sln
```

### Publicação no Linux com Coolify (recomendado)

Deploy via **Dockerfile** na raiz do repositório. Coolify faz build e run automático a cada push no Git.

**Guia completo:** `docs/dev-history/2026-06-08_deploy-coolify-linux.md`  
**Checklist passo a passo:** `docs/deploy-coolify-checklist.md`

**Resumo no Coolify:**

| Configuração | Valor |
|--------------|-------|
| Build pack | Dockerfile (`/Dockerfile`) |
| Porta exposta | `8080` |
| Health check | `/health` |
| Volume persistente | `/app/keys` |
| Auto deploy | Webhook Git (push na branch `main`) |

**Variáveis de ambiente** (ver `.env.example`):

```text
ConnectionStrings__DefaultConnection=Server=IP,1433;Database=mmoneyweb;...
ConnectionStrings__MMoneyConnection=Server=IP,1433;Database=mmoneyweb;...
Database__RunMigrationsOnStartup=false
App__RequireHttps=false
```

No **primeiro deploy**, use `Database__RunMigrationsOnStartup=true` para aplicar migrations do Identity; depois volte para `false`.

**Teste local da imagem:**

```bash
cp .env.example .env
docker compose up --build
```

**Requisitos:** SQL Server acessível na porta `1433` a partir do IP do servidor Coolify; domínio com DNS apontando para o servidor; HTTPS e WebSocket gerenciados pelo Traefik do Coolify (obrigatório para Blazor Server).

### Publicação no Windows Server com IIS (legado)

```powershell
dotnet publish src\MMoneyWeb.Web\MMoneyWeb.Web.csproj -c Release -o .\publish
```

Copie na pasta `publish` o arquivo `appsettings.Production.json` (use `appsettings.Production.example.json` como modelo; não commitar senhas).

**Script de configuracao no servidor (pool, permissoes, web.config, bindings):**

```powershell
.\scripts\configure-iis-mmoneyweb.ps1
```

Execute como **Administrator** no EC2, com os arquivos publicados em `C:\inetpub\vhosts\mmoneyweb.com`.

**Importante:** este projeto é **ASP.NET Core (.NET 10)**, não .NET Framework 4.7. Os apps legados no mesmo IIS podem continuar em pools **v4.0**; o MMoneyWeb **precisa de pool e módulo separados**.

**Requisitos no servidor:**

1. IIS habilitado
2. Recurso **WebSocket Protocol** instalado (obrigatório para Blazor Server)
3. **[.NET 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)** instalado e IIS reiniciado (`iisreset`)
4. Verificar módulo: deve existir `C:\Program Files\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll`
5. **Application Pool dedicado** para o MMoneyWeb:
   - **.NET CLR version:** `No Managed Code` (Sem código gerenciado)
   - Não reutilizar o pool dos apps .NET Framework 4.7
6. Site/aplicativo IIS apontando para a pasta `publish` (com `web.config` e `MMoneyWeb.Web.dll`)
7. Permissão de leitura/execução na pasta para o usuário do pool (ex.: `IIS AppPool\NomeDoPool`)
8. Pasta `logs` na publicação com permissão de escrita (stdout do `web.config`)
9. **HTTPS** configurado antes de qualquer acesso externo

**Connection strings em produção:** `appsettings.Production.json` na pasta publicada, ou variáveis no IIS (Configuration Editor → `system.webServer/aspNetCore/environmentVariables`).

**Migrations Identity (uma vez no servidor):**

```powershell
dotnet ef database update --project src\MMoneyWeb.Web --context ApplicationDbContext
```

**Erros comuns no IIS**

| Sintoma | Causa provável | Ação |
|--------|----------------|------|
| HTTP 500.19 — módulo AspNetCoreModuleV2 ausente | Hosting Bundle não instalado | Instalar .NET 10 Hosting Bundle e `iisreset` |
| HTTP 502.5 — process failure | Connection string inválida ou runtime ausente | Conferir `appsettings.Production.json` e logs em `publish\logs\stdout_*.log` |
| Página em branco / circuito Blazor cai | WebSocket desabilitado | Instalar **WebSocket Protocol** no IIS |
| App .NET 4.7 funciona, este não | Pool com CLR v4.0 | Criar pool novo com **No Managed Code** |

## Segurança — registro público

Após criar o **primeiro usuário**, o **registro público deve ser desativado** antes de publicar em ambiente acessível pela internet (remover ou restringir a rota `Account/Register` e/ou desabilitar a política de registro no Identity).

## Próximos passos sugeridos

1. Definir modelo de domínio financeiro em `Domain/` e mapear em `MMoneyDbContext`
2. Criar migration inicial do `MMoneyDbContext`
3. Implementar serviços em `Services/` (sem repository genérico)
4. Desativar registro público após primeiro usuário
5. Configurar connection strings seguras no IIS para produção
6. Implementar cadastros e telas de controle financeiro

## Documentação para IA / Cursor

- Regras do agente: `.cursor/rules/2026_06_07_mmoney-web.mdc`
- Índice de memória: `.cursor/context/2026_06_07_indice-memoria-ia.md`
