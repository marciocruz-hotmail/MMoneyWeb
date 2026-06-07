# CHANGELOG-DEV.md

> Changelog **operacional** (alvo: ≤200 linhas).  
> **Histórico integral:** `docs/dev-history/`  
> **Contexto fixo:** `AI-CONTEXT.md` | **Pendências:** `BACKLOG-DEV.md` | **Índice temas:** `.cursor/context/2026_06_07_indice-memoria-ia.md`

**Última atualização:** 2026-06-07 — deploy IIS + view Lançamentos

---

## Estado atual do projeto

Monólito **ASP.NET Core Blazor Web App** (.NET 10, Interactive Server) com **ASP.NET Core Identity**, **EF Core 10** e SQL Server. View **Lançamentos** com grid, filtros, modal create/edit e persistência no banco legado `mmoneyweb`. Deploy alvo: IIS em `C:\inetpub\vhosts\mmoneyweb.com` (EC2); app validado em Kestrel (HTTP 200); **IIS em ajuste final** via `scripts/configure-iis-mmoneyweb.ps1`.

**Build** e **testes** xUnit OK.

---

## Decisões técnicas ativas

- **.NET 10 LTS** + Blazor Interactive Server — monólito simples.
- **Sem** microsserviços, Docker, Aspire, API REST, MediatR, AutoMapper, FluentValidation, Serilog, Dapper, libs UI externas, repository genérico.
- SQL Server: `DefaultConnection` (Identity), `MMoneyConnection` (`MMoneyDbContext`).
- `MMoneyDbContext` apenas via `IDbContextFactory` em Blazor.
- Área principal: `[Authorize]` em `Components/Pages/_Imports.razor`.
- Publicação alvo: IIS + .NET 10 Hosting Bundle; secrets fora do repo.
- Agente: sem `git push` / deploy remoto; português BR.

---

## Últimas alterações relevantes

### 2026-06-07 — Deploy IIS (sessão EC2)
- Servidor: site `mmoneyweb.com` (id 4), pool dedicado sem CLR, path `C:\inetpub\vhosts\mmoneyweb.com`.
- Pré-requisito: **.NET 10 Hosting Bundle** (não SDK); módulo `AspNetCoreModuleV2`.
- App OK em Kestrel (`dotnet MMoneyWeb.Web.dll` → HTTP 200 login); IIS 500/503 por pool v4.0, binding só `www`, `processPath` sem caminho completo, permissões `logs`/`keys`.
- Código: `App:RequireHttps` opcional, Data Protection em `keys/`, `web.config` IIS, publish sem arquivos Development.
- Scripts: `configure-iis-mmoneyweb.ps1`, `publish-iis.ps1`, `iis-diagnostico.ps1`.
- Histórico completo: `docs/dev-history/2026-06-07_deploy-iis-mmoneyweb.md`.

### 2026-06-07 — View Lançamentos + modal create/edit
- Grid com saldo, cores por status/valor, edição inline; `LancamentosViewService`, entidades legadas, `CreateEditLancamentoModal`.

### 2026-06-07 — Fundação visual AdminLTE 4 + Bootstrap 5
- Shell administrativo: `AppHeader`, `AppSidebar`, `AppFooter`, `MainLayout`, `AuthLayout`.
- Assets mínimos em `wwwroot/lib/vendor/` (adminlte, overlayscrollbars, bootstrap-icons).
- Dashboard + placeholders (Contas, Lançamentos, Categorias, Cartões, Relatórios); CSS/JS em `wwwroot/css/` e `wwwroot/js/`.

### 2026-06-07 — SQL Server remoto e login Identity
- Connection strings em `appsettings.Development.json` (gitignored) apontando para `98.90.225.8` / `mmoneyweb` com autenticação SQL (TCP `,1433`).
- `RequireConfirmedAccount = false` (sem envio real de e-mail no template).
- Migration Identity aplicada no servidor (`AspNet*`); `appsettings.Development.example.json` como modelo.

### 2026-06-07 — Fundação técnica Blazor Web App
- Solution `MMoneyWeb.sln`, `src/MMoneyWeb.Web`, `tests/MMoneyWeb.Tests`.
- Template Blazor Individual auth + SQL Server; `MMoneyDbContext` + factory; pastas `Domain/`, `Services/`.
- Home mínima; removidos Counter, Weather, Auth demo; NavMenu simplificado.
- `README.md`, `.gitignore`, repositório Git local; docs IA alinhadas à stack.

### 2026-06-07 — Estrutura documental e regras Cursor
- Criados `AI-CONTEXT.md`, `BACKLOG-DEV.md`, `.cursor/rules/`, `.cursor/context/`, `.cursor/governanca/`, `.claude/`, `docs/dev-history/`.

---

## Pendências abertas

Ver **`BACKLOG-DEV.md`**.

---

## Histórico completo

Entradas antigas ou detalhe extenso → `docs/dev-history/`.
