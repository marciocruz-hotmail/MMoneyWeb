# docs/dev-history/

Arquivo de **histórico** do projeto MMoneyWeb.

## Entradas

| Data | Tema | Arquivo |
|------|------|---------|
| 2026-06-08 | Deploy Linux + Coolify (Docker, Git CI/CD) | [2026-06-08_deploy-coolify-linux.md](./2026-06-08_deploy-coolify-linux.md) |
| 2026-06-07 | Deploy IIS EC2 (`mmoneyweb.com`) | [2026-06-07_deploy-iis-mmoneyweb.md](./2026-06-07_deploy-iis-mmoneyweb.md) |

## Uso

| Tipo de conteúdo | Onde colocar |
|------------------|--------------|
| Changelog integral antes da compactação | `CHANGELOG-DEV-HISTORICO-INICIAL.md` |
| Entradas antigas movidas do changelog operacional | `CHANGELOG-DEV-YYYY-MM-DD.md` ou append no histórico inicial |
| Relatórios de auditoria datados | `RELATORIO-YYYY-MM-DD-<tema>.md` |

## Regras

- **Não editar** estes ficheiros no dia a dia — apenas append ou novos ficheiros datados.
- O changelog **operacional** compacto fica na raiz: `CHANGELOG-DEV.md`.
- Detalhe técnico por tema fica em `.cursor/context/AAAA_MM_DD_*.md`.

## Referências

- Índice memória: `.cursor/context/2026_06_07_indice-memoria-ia.md`
- Contexto fixo: `AI-CONTEXT.md`
