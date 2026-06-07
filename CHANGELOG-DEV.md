# CHANGELOG-DEV.md

> Changelog **operacional** (alvo: ≤200 linhas).  
> **Histórico integral:** `docs/dev-history/`  
> **Contexto fixo:** `AI-CONTEXT.md` | **Pendências:** `BACKLOG-DEV.md` | **Índice temas:** `.cursor/context/2026_06_07_indice-memoria-ia.md`

**Última atualização:** 2026-06-07 — fundação visual AdminLTE 4

---

## Estado atual do projeto

Monólito **ASP.NET Core Blazor Web App** (.NET 10, Interactive Server) com **ASP.NET Core Identity**, **EF Core 10** e SQL Server. Dois contextos: `ApplicationDbContext` (Identity) e `MMoneyDbContext` (financeiro, via `AddDbContextFactory`). Página inicial mínima com auth; páginas demo removidas. **Sem** funcionalidades de negócio financeiro.

**Build** e **testes** xUnit OK na fundação.

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
