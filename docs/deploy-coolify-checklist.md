# Checklist Coolify — MMoneyWeb

Guia operacional **item a item** para o primeiro deploy e CI/CD automático.  
Valores do projeto atual; substitua o que estiver marcado com `⚠️`.

| Parâmetro | Valor do projeto |
|-----------|------------------|
| Domínio | `mmoneyweb.com` e `www.mmoneyweb.com` |
| SQL Server | `98.90.225.8,1433` |
| Database | `mmoneyweb` |
| App (container) | porta `8080`, health `/health` |
| Branch produção | `master` (repo: `marciocruz-hotmail/MMoneyWeb`) |

---

## Fase 0 — Pré-requisitos (antes de abrir o Coolify)

- [ ] **0.1** Servidor Linux com Coolify instalado e acessível (`https://COOLIFY-PAINEL ⚠️`)
- [ ] **0.2** Anotar o **IP público do servidor Coolify**: `_______________` ⚠️
- [ ] **0.3** Repositório Git criado (GitHub/GitLab/Gitea): URL `_______________` ⚠️
- [ ] **0.4** Git instalado na máquina de desenvolvimento (`git --version`)
- [ ] **0.5** Código publicado na branch `master` (ver `scripts/init-git-repo.ps1`)

---

## Fase 1 — SQL Server (EC2 Windows `98.90.225.8`)

O container Linux precisa alcançar o SQL Server já usado no IIS.

- [ ] **1.1** Confirmar SQL Server escutando em TCP **1433** (SQL Server Configuration Manager)
- [ ] **1.2** No **Security Group / firewall do EC2 Windows**, liberar entrada:
  - Porta: `1433`
  - Origem: **IP do servidor Coolify** (fase 0.2) — não usar `0.0.0.0/0` em produção
- [ ] **1.3** Autenticação **SQL Server** habilitada (modo misto)
- [ ] **1.4** Usuário SQL com acesso ao banco `mmoneyweb` (anotar credenciais em local seguro, **não** no Git)
- [ ] **1.5** Teste de conectividade **a partir do servidor Coolify** (SSH no Linux):

```bash
# Se tiver mssql-tools ou usar telnet/nc:
nc -zv 98.90.225.8 1433
# Deve responder "succeeded" ou equivalente
```

- [ ] **1.6** Montar connection string (usar no Coolify):

```text
Server=98.90.225.8,1433;Database=mmoneyweb;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

---

## Fase 2 — Repositório Git + CI

- [ ] **2.1** Executar na pasta do projeto:

```powershell
.\scripts\init-git-repo.ps1
```

- [ ] **2.2** Criar repositório vazio no GitHub/GitLab (sem README, sem .gitignore)
- [ ] **2.3** Associar remote e publicar:

```powershell
git remote add origin https://github.com/marciocruz-hotmail/MMoneyWeb.git
git push -u origin master
```

- [ ] **2.4** Verificar GitHub Actions: workflow **CI** verde (build + test + docker build)
- [ ] **2.5** Confirmar que **não** há secrets no repositório (`appsettings.Production.json`, `.env` estão no `.gitignore`)

---

## Fase 3 — Nova aplicação no Coolify

Painel Coolify → **+ New** → **Resource** → **Application**

### 3.1 Source (Git)

| Campo | Valor |
|-------|-------|
| Source | GitHub / GitLab / Gitea |
| Repository | `SEU_USUARIO/MMoneyWeb` ⚠️ |
| Branch | `master` |
| Build Pack | **Dockerfile** |
| Dockerfile location | `/Dockerfile` |
| Base directory | `/` (raiz) |

- [ ] **3.1** Repositório conectado e branch `main` selecionada
- [ ] **3.2** Build pack = Dockerfile (não Nixpacks)

### 3.2 Network

| Campo | Valor |
|-------|-------|
| Port (exposes) | `8080` |
| Health check enabled | **Sim** |
| Health check path | `/health` |
| Health check port | `8080` |

- [ ] **3.3** Porta `8080` configurada
- [ ] **3.4** Health check em `/health`

### 3.3 Domínios

| Domínio | HTTPS |
|---------|-------|
| `mmoneyweb.com` | Let's Encrypt ✅ |
| `www.mmoneyweb.com` | Let's Encrypt ✅ |

- [ ] **3.5** Domínios adicionados na aplicação Coolify
- [ ] **3.6** DNS: registros **A** (ou CNAME) apontando para **IP do servidor Coolify** ⚠️
- [ ] **3.7** Aguardar emissão do certificado SSL (pode levar alguns minutos)

### 3.4 Persistent Storage (obrigatório)

| Name | Mount path |
|------|------------|
| `mmoneyweb-keys` | `/app/keys` |

- [ ] **3.8** Volume persistente criado em `/app/keys`

Sem este volume, cookies de login expiram a cada redeploy.

### 3.5 Environment Variables

Copiar no painel **Environment Variables** do Coolify:

| Variável | Valor | Build time? | Observação |
|----------|-------|-------------|------------|
| `ConnectionStrings__DefaultConnection` | connection string completa (**credenciais reais**, não `SEU_USUARIO`) | Não | Identity |
| `ConnectionStrings__MMoneyConnection` | **mesma string** que DefaultConnection | Não | Dados financeiros |
| `Database__RunMigrationsOnStartup` | `true` | Não | **Só no 1º deploy** |
| `App__RequireHttps` | `false` | Não | TLS no Traefik |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Não | Já no Dockerfile |

- [ ] **3.9** Variáveis configuradas (marcar como **secret** no Coolify se disponível)
- [ ] **3.9b** **Desmarcar** "Use Docker Build Secrets" para connection strings (só runtime, não build)
- [ ] **3.9c** `ConnectionStrings__DefaultConnection` e `ConnectionStrings__MMoneyConnection` com **a mesma** connection string válida
- [ ] **3.10** `Database__RunMigrationsOnStartup=true` apenas no primeiro deploy

### 3.6 Auto Deploy

| Campo | Valor |
|-------|-------|
| Automatic Deployment | **Enabled** |
| Webhook / Deploy on push | **Enabled** |
| Watch paths | (padrão — todo o repo) |

- [ ] **3.11** Auto deploy ativado
- [ ] **3.12** Webhook registrado no GitHub/GitLab (Coolify faz isso ao conectar)

---

## Fase 4 — Primeiro deploy

- [ ] **4.1** Clicar **Deploy** (ou aguardar push automático)
- [ ] **4.2** Acompanhar logs de build — deve concluir `dotnet publish` sem erro
- [ ] **4.3** Container status **healthy** (health check `/health` OK)
- [ ] **4.4** Acessar `https://mmoneyweb.com/health` → `{"status":"healthy"}`
- [ ] **4.5** Acessar `https://mmoneyweb.com` → tela de login
- [ ] **4.6** Fazer login com usuário existente **ou** criar o primeiro usuário (registro público já removido no código — usar fluxo existente/admin)
- [ ] **4.7** Navegar para **Lançamentos** — confirmar grid carrega (testa `MMoneyConnection`)
- [ ] **4.8** Deixar página aberta 2–3 min — circuito Blazor **não** deve cair (WebSocket)
- [ ] **4.9** Alterar `Database__RunMigrationsOnStartup` para `false` e **redeploy**
- [ ] **4.10** Redeploy de teste — login deve **permanecer** válido (volume `/app/keys`)

---

## Fase 5 — Cutover DNS (quando Coolify estiver validado)

Se o domínio ainda aponta para o EC2 Windows (IIS):

- [ ] **5.1** Coolify 100% validado na fase 4
- [ ] **5.2** Reduzir TTL do DNS com antecedência (ex.: 300s)
- [ ] **5.3** Alterar registro A de `mmoneyweb.com` e `www` → **IP Coolify**
- [ ] **5.4** Aguardar propagação; testar HTTPS em ambos os hostnames
- [ ] **5.5** (Opcional) Parar site IIS `mmoneyweb.com` no EC2 Windows
- [ ] **5.6** Marcar **M-OPS-09** concluído no `BACKLOG-DEV.md`

---

## Fase 6 — Pós-produção

- [ ] **6.1** Confirmar que não existe rota pública de registro (`/Account/Register` — já ausente)
- [ ] **6.2** Backup do volume `/app/keys` (política Coolify ou snapshot do servidor)
- [ ] **6.3** Monitorar logs do container nas primeiras 24h
- [ ] **6.4** Documentar URL do painel Coolify e credenciais em cofre (1Password, etc.)

---

## Fluxo contínuo (após setup)

```text
git commit → git push origin master
    → GitHub Actions (CI: build + test)
    → Coolify webhook (rebuild + restart)
    → https://mmoneyweb.com atualizado
```

---

## Troubleshooting rápido

| Problema | Verificar |
|----------|-----------|
| Build falha no Coolify | Logs de build; .NET 10 no Dockerfile |
| Container unhealthy | Connection string; SQL acessível na 1433 |
| 502 Bad Gateway | Porta exposta = 8080 |
| Login OK, Lançamentos erro | `MMoneyConnection` igual à `DefaultConnection` |
| Blazor desconecta | WebSocket no Traefik; não usar CDN que quebre WSS |
| Logout após redeploy | Volume `/app/keys` ausente ou não montado |

---

## Referências

- Guia técnico: `docs/dev-history/2026-06-08_deploy-coolify-linux.md`
- Variáveis: `.env.example`
- Script Git: `scripts/init-git-repo.ps1`
