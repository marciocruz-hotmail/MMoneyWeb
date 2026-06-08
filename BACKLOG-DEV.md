# BACKLOG-DEV.md — Plano de implementação (checklist)

Pendências **ativas** do **MMoneyWeb**. Itens concluídos: registo em `CHANGELOG-DEV.md`.

**Como usar:** marcar `[x]` só quando o critério de aceite estiver cumprido. Sem `git push` nem deploy remoto salvo pedido explícito.

**Índice de memória:** `.cursor/context/2026_06_07_indice-memoria-ia.md`

**Referências**

| Tema | Documento |
|------|-----------|
| Arquitetura monólito Blazor | `.cursor/context/2026_06_07_arquitetura-blazor-monolito.md` |
| Comandos e IIS | `README.md` |

---

## Legenda de IDs

| Prefixo | Grupo |
|---------|-------|
| **M-INIT** | Arranque / fundação |
| **M-ARCH** | Arquitetura e dados |
| **M-FEAT** | Funcionalidades financeiras |
| **M-SEC** | Segurança |
| **M-OPS** | Deploy e ambiente |

---

## Alta prioridade

- [ ] **M-OPS-05** — Executar `scripts/init-git-repo.ps1`, publicar em remote Git e conectar ao Coolify (auto-deploy on push)
- [ ] **M-OPS-06** — Primeiro deploy Coolify: env vars, volume `/app/keys`, `Database__RunMigrationsOnStartup=true` (uma vez)
- [ ] **M-OPS-07** — Liberar SQL Server (porta 1433) para IP do servidor Coolify; validar login em produção
- [ ] **M-OPS-08** — Domínio + HTTPS no Coolify; confirmar Blazor Server (WebSocket) estável
- [ ] **M-SEC-01** — Desativar registro público após criação do primeiro usuário (antes de produção)

---

## Média prioridade

- [ ] **M-OPS-04** — HTTPS Coolify (Let's Encrypt) validado; opcional `App.RequireHttps: true` se terminar TLS no app
- [ ] **M-OPS-09** — IIS legado: decidir descomissionar ou manter paralelo até cutover Coolify
- [ ] **M-ARCH-01** — Entidades `Domain/` adicionais e migrations se necessário (tabelas legadas já mapeadas)
- [ ] **M-FEAT-02** — Implementar serviços em `Services/` (sem repository genérico)

---

## Baixa prioridade

- [x] **M-OPS-10** — Pipeline CI GitHub Actions (build + test + docker build) — 2026-06-08
- [ ] **M-DOC-01** — ADRs adicionais em `.cursor/context/` por padrão estabelecido

---

## Itens em análise

- [ ] **M-FEAT-03** — Relatórios e dashboard (escopo futuro)

---

## Concluídos recentemente

- [x] **M-INIT-01** — Fundação .NET 10 Blazor + Identity + EF Core + xUnit (2026-06-07)
- [x] **M-DOC-00** — Estrutura memória/regras Cursor + alinhamento à stack (2026-06-07)
