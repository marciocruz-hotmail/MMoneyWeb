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

- [ ] **M-ARCH-01** — Modelar entidades em `Domain/` e primeira migration do `MMoneyDbContext`
- [ ] **M-SEC-01** — Desativar registro público após criação do primeiro usuário (antes de produção)
- [ ] **M-OPS-01** — Configurar connection strings seguras (User Secrets dev / IIS produção)

---

## Média prioridade

- [ ] **M-FEAT-01** — Definir escopo do primeiro módulo financeiro (contas, categorias, lançamentos)
- [ ] **M-FEAT-02** — Implementar serviços em `Services/` (sem repository genérico)

---

## Baixa prioridade

- [ ] **M-OPS-02** — Pipeline CI (build + test) local ou GitHub Actions
- [ ] **M-DOC-01** — ADRs adicionais em `.cursor/context/` por padrão estabelecido

---

## Itens em análise

- [ ] **M-FEAT-03** — Relatórios e dashboard (escopo futuro)

---

## Concluídos recentemente

- [x] **M-INIT-01** — Fundação .NET 10 Blazor + Identity + EF Core + xUnit (2026-06-07)
- [x] **M-DOC-00** — Estrutura memória/regras Cursor + alinhamento à stack (2026-06-07)
