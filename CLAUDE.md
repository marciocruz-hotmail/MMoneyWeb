# CLAUDE.md — MMoneyWeb 

Controle financeiro pessoal | .NET 10 | Blazor Web App (Interactive Server) | SQL Server + EF Core 10 | Identity  
Respostas ao utilizador: **português brasileiro**.

@.claude/CHANGELOG-RECENT.md

**Memória:** `AI-CONTEXT.md` → `CHANGELOG-DEV.md` → `BACKLOG-DEV.md` → `.cursor/context/2026_06_07_indice-memoria-ia.md`.  
**Regras agente:** `.cursor/rules/2026_06_07_mmoney-web.mdc` — **não duplicar aqui**.

---

## Padrões — ponteiro (detalhe no índice)

| Tema | Contexto |
|------|----------|
| **Arquitetura monólito Blazor** | `2026_06_07_arquitetura-blazor-monolito.md` |
| DbContext Identity vs financeiro | `AI-CONTEXT.md` § Arquitetura |
| Comandos build/test/publish | `README.md` |

---

## Ficheiros críticos

- `src/MMoneyWeb.Web/Program.cs` — DI e pipeline
- `src/MMoneyWeb.Web/Data/MMoneyDbContext.cs` — contexto financeiro (factory)
- `src/MMoneyWeb.Web/Data/ApplicationDbContext.cs` — Identity
- `src/MMoneyWeb.Web/Components/Routes.razor` — autorização de rotas
- `src/MMoneyWeb.Web/Components/Pages/_Imports.razor` — `[Authorize]` área principal

---

## Armadilhas

- Injetar `MMoneyDbContext` scoped em componente Blazor → usar `IDbContextFactory<MMoneyDbContext>`
- Connection strings com credenciais reais no repo → User Secrets / IIS / variáveis de ambiente
- Registro público ativo em produção → desativar após primeiro usuário
- Adicionar MediatR, Dapper, libs UI externas → fora do escopo atual

---

## Verificação rápida

```powershell
dotnet build MMoneyWeb.sln
dotnet test MMoneyWeb.sln
```

Mais inventários → índice memória § Scripts.
