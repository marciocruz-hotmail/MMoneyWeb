# Deploy Linux + Coolify — MMoneyWeb

**Data:** 2026-06-08  
**Alvo:** servidor Linux com [Coolify](https://coolify.io/) (proxy Traefik), deploy automático via Git.

---

## Arquitetura de implantação

```text
┌─────────────┐     push      ┌──────────────┐     build      ┌─────────────────┐
│  Git remote │ ────────────► │   Coolify    │ ─────────────► │ Docker image    │
│ (GitHub/…)  │               │  (webhook)   │   Dockerfile   │ aspnet:10.0     │
└─────────────┘               └──────┬───────┘                └────────┬────────┘
                                     │ HTTPS/WSS                       │ TCP 1433
                                     ▼                                 ▼
                              ┌──────────────┐                ┌─────────────────┐
                              │   Traefik    │                │  SQL Server     │
                              │ (Coolify)    │                │  (externo)      │
                              └──────────────┘                └─────────────────┘
```

| Componente | Responsabilidade |
|------------|------------------|
| **Git** | Fonte de verdade do código; push dispara rebuild no Coolify |
| **Coolify** | Clone, `docker build`, run do container, SSL, domínio, volumes |
| **Traefik** | Terminação TLS, roteamento HTTP/WebSocket (obrigatório para Blazor Server) |
| **Container** | Kestrel na porta `8080`, chaves Data Protection em `/app/keys` |
| **SQL Server** | Banco existente `mmoneyweb` (Identity + dados financeiros legados) |

### Decisões

1. **Monólito Blazor** permanece — sem microsserviços nem API separada.
2. **SQL Server externo** — o container não embute banco; reutiliza instância já existente.
3. **Secrets via variáveis de ambiente** no Coolify (nunca no Git).
4. **Volume persistente** em `/app/keys` — cookies Identity e antiforgery sobrevivem a redeploys.
5. **`App:RequireHttps: false`** no container — HTTPS é responsabilidade do Traefik.
6. **Forwarded Headers** no `Program.cs` — o app reconhece o esquema/host real do proxy.

---

## Pré-requisitos

| Item | Detalhe |
|------|---------|
| Servidor Linux | Docker instalado (Coolify provisiona) |
| Coolify | Instância acessível com projeto criado |
| Repositório Git | GitHub, GitLab, Gitea ou similar |
| SQL Server | Porta `1433` liberada para o IP do servidor Coolify |
| Domínio | DNS apontando para o IP do servidor (ex.: `mmoneyweb.com`) |

---

## Ficheiros de deploy no repositório

| Ficheiro | Função |
|----------|--------|
| `Dockerfile` | Build multi-stage .NET 10 → imagem `aspnet:10.0` |
| `.dockerignore` | Contexto de build enxuto |
| `docker-compose.yml` | Teste local antes do Coolify |
| `.env.example` | Modelo de variáveis (Coolify ou `docker compose`) |
| `.github/workflows/ci.yml` | Build + testes em cada push/PR |

---

## Configuração no Coolify (passo a passo)

### 1. Criar aplicação

1. **New Resource** → **Application**
2. **Source:** repositório Git (conectar conta GitHub/GitLab)
3. **Build Pack:** `Dockerfile`
4. **Dockerfile location:** `/Dockerfile` (raiz do repo)
5. **Port:** `8080`
6. **Health check path:** `/health`

### 2. Variáveis de ambiente

Configurar no painel **Environment Variables** (ver `.env.example`):

| Variável | Exemplo / nota |
|----------|----------------|
| `ConnectionStrings__DefaultConnection` | Identity — SQL Server remoto |
| `ConnectionStrings__MMoneyConnection` | Dados financeiros |
| `Database__RunMigrationsOnStartup` | `true` **somente no primeiro deploy** |
| `App__RequireHttps` | `false` |
| `ASPNETCORE_ENVIRONMENT` | `Production` (já definido no Dockerfile) |

### 3. Volume persistente

| Mount path | Finalidade |
|------------|------------|
| `/app/keys` | Chaves ASP.NET Data Protection |

Sem este volume, usuários serão deslogados a cada redeploy.

### 4. Domínio e SSL

1. Adicionar domínio na aplicação Coolify
2. Ativar **HTTPS** (Let's Encrypt automático)
3. Confirmar que **WebSocket** está habilitado (padrão no Traefik do Coolify)

### 5. Deploy automático

1. Em **Webhooks / Auto Deploy:** ativar deploy on push
2. Branch de produção: `main` (ou a branch acordada)
3. Cada `git push` → Coolify rebuild + rolling restart

### 6. Primeiro deploy

1. Definir `Database__RunMigrationsOnStartup=true`
2. Deploy e aguardar container healthy (`/health`)
3. Acessar URL, criar primeiro usuário
4. **Desativar registro público** (ver `M-SEC-01` no backlog)
5. Voltar `Database__RunMigrationsOnStartup=false` e redeploy

---

## Teste local (antes do Coolify)

```bash
cp .env.example .env
# editar .env com connection strings reais

docker compose up --build
# http://localhost:8080/health → {"status":"healthy"}
```

---

## Firewall SQL Server

O servidor Linux (IP público do Coolify) precisa alcançar o SQL Server:

- Liberar porta **1433** no security group / firewall do SQL Server
- Autenticação SQL habilitada no SQL Server
- Connection string com `Encrypt=True;TrustServerCertificate=True`

---

## Migração IIS → Coolify

| IIS (Windows) | Coolify (Linux) |
|---------------|-----------------|
| Hosting Bundle + `web.config` | `Dockerfile` + imagem `aspnet` |
| Pool IIS + WebSocket Protocol | Traefik + WebSocket automático |
| `appsettings.Production.json` no servidor | Variáveis de ambiente no Coolify |
| `keys/` em `C:\inetpub\...` | Volume `/app/keys` |
| Scripts `.ps1` | `docker compose` / painel Coolify |

Scripts IIS (`configure-iis-mmoneyweb.ps1`, etc.) permanecem no repo como referência histórica.

---

## Troubleshooting

| Sintoma | Causa provável | Ação |
|---------|----------------|------|
| Container unhealthy | SQL inacessível ou app crash na subida | Logs Coolify; testar connection string |
| HTTP 502 | Porta errada no Coolify | Confirmar porta `8080` |
| Blazor desconecta / circuito cai | WebSocket bloqueado | Verificar proxy Coolify; não usar CDN que quebre WSS |
| Login invalida após redeploy | Volume `/app/keys` ausente | Adicionar persistent storage |
| Erro de migration | `RunMigrationsOnStartup` com DB já migrado | Manter `false` após primeiro deploy |

---

## Referências

- `README.md` — secção Coolify
- `.env.example` — variáveis
- `AI-CONTEXT.md` — resumo operacional
