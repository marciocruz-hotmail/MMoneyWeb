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
| Hospedagem alvo | Windows Server + IIS |

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
- (Publicação) Windows Server com IIS e [.NET 10 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/10.0)

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

### Publicação no Windows Server com IIS

```powershell
dotnet publish src\MMoneyWeb.Web\MMoneyWeb.Web.csproj -c Release -o .\publish
```

**Requisitos no servidor:**

1. IIS habilitado
2. **.NET 10 Hosting Bundle** instalado
3. Application Pool configurado como **No Managed Code**
4. Site IIS apontando para a pasta `publish`
5. **HTTPS** configurado antes de qualquer acesso externo

Configure as connection strings no IIS (variáveis de ambiente ou `appsettings.Production.json` fora do repositório).

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
