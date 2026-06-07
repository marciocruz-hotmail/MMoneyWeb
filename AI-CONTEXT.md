# AI-CONTEXT.md

Contexto técnico **fixo** do **MMoneyWeb** para IA e developers. Detalhe por tema → **`.cursor/context/2026_06_07_indice-memoria-ia.md`**.

| Ficheiro | Função |
|----------|--------|
| **`CHANGELOG-DEV.md`** | Estado atual + últimas alterações (compacto) |
| **`BACKLOG-DEV.md`** | Pendências ativas (IDs M-*) |
| **Índice temas** | `.cursor/context/2026_06_07_indice-memoria-ia.md` |
| **Regras agente** | `.cursor/rules/2026_06_07_mmoney-web.mdc` |
| **Histórico** | `docs/dev-history/` |

---

## Projeto

**MMoneyWeb** — aplicativo web **pessoal** de controle financeiro. Monólito Blazor em fase de fundação; **sem** funcionalidades de negócio implementadas ainda.

**Fonte de verdade:** em conflito entre documentação e código (`MMoneyWeb.Web.csproj`, `Program.cs`, estrutura real), **prevalece o repositório**.

---

## Stack

| Camada | Tecnologia |
|--------|------------|
| Runtime | .NET 10 LTS |
| UI | ASP.NET Core Blazor Web App — **Interactive Server** |
| Dados | SQL Server + EF Core 10 |
| Auth | ASP.NET Core Identity (contas individuais) |
| Testes | xUnit (`tests/MMoneyWeb.Tests`) |
| Hospedagem alvo | Windows Server + IIS (.NET 10 Hosting Bundle) |

**Não usar nesta fase:** microsserviços, Docker, Aspire, API REST, MediatR, AutoMapper, FluentValidation, Serilog, Dapper, libs externas de UI, repository genérico.

---

## Ordem de leitura (antes de código)

1. Este ficheiro → `CHANGELOG-DEV.md` → `BACKLOG-DEV.md`
2. Arquitetura → `.cursor/context/2026_06_07_arquitetura-blazor-monolito.md`
3. Tema específico → **índice memória** → ficheiro `.cursor/context/`
4. Formato da resposta → `.mdc` §6

---

## Arquitetura (resumo)

| Contexto | Connection string | Registo DI |
|----------|-------------------|------------|
| `ApplicationDbContext` | `DefaultConnection` | `AddDbContext` (Identity) |
| `MMoneyDbContext` | `MMoneyConnection` | `AddDbContextFactory` |

Em Blazor: **`IDbContextFactory<MMoneyDbContext>`** — não ciclo de vida prolongado do contexto financeiro em componentes.

Detalhe: `.cursor/context/2026_06_07_arquitetura-blazor-monolito.md`.

---

## Áreas sensíveis

| Área | Nota |
|------|------|
| `Program.cs` | DI, pipeline, auth |
| `Data/` + `Migrations/` | Identity e futuro schema financeiro |
| `Components/Account/` | Identity — registro público desativar antes de produção |
| Connection strings | Nunca commitar credenciais reais; usar env/IIS/User Secrets |
| `Components/Pages/_Imports.razor` | `[Authorize]` na área principal |

---

## Registo pós-intervenção

1. Linha em `CHANGELOG-DEV.md` § «Últimas alterações relevantes»
2. `BACKLOG-DEV.md` — marcar pendência
3. Detalhe longo → `.cursor/context/AAAA_MM_DD_*.md`

**Convenção nomes:** `AAAA_MM_DD_` — ver `.mdc` §4.5.
