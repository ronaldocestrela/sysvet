# Configuração e Observabilidade (SysVet)

Este documento descreve as decisões arquiteturais relacionadas à configuração da aplicação, observabilidade e verificações de integridade.

## 1. Configuração por ambiente

### Precedência de fontes

A API ASP.NET Core carrega configuração nesta ordem (valores posteriores sobrescrevem anteriores):

1. `appsettings.json`
2. `appsettings.{Environment}.json` (ex.: `Development`, `Staging`, `Production`)
3. **User Secrets** — apenas quando `ASPNETCORE_ENVIRONMENT=Development` e o projeto define `UserSecretsId` ([`src/API/API.csproj`](../../src/API/API.csproj))
4. Variáveis de ambiente e argumentos de linha de comando

### Options tipados e fail-fast

As configurações não são lidas diretamente de `IConfiguration` na lógica de negócio. Utilizamos o **Options Pattern** com `ValidateDataAnnotations()` e `ValidateOnStart()` (registro via `AddValidatedOptions<T>()` em [`Core.Infrastructure/Configuration`](../../src/Modules/Core/Infrastructure/Configuration/)).

Propriedades críticas (`JwtSettings.Secret`, `TenancySettings.DefaultSchema`, `Database.Provider`, etc.) possuem anotações `[Required]`, `[MinLength]`, etc. Se a aplicação inicializar com configuração inválida ou incompleta para o ambiente, o host **falha no boot** (fail-fast).

### Connection strings e banco de dados

Todos os DbContexts de módulo (Core, Veterinary, Inventory, Sales) resolvem a connection string via [`ModuleConnectionStringResolver`](../../src/Modules/Core/Infrastructure/Configuration/ModuleConnectionStringResolver.cs):

- Padrão: `ConnectionStrings:{Database:ConnectionStringName}` (nome padrão: `DefaultConnection`)
- Override opcional por módulo: `{Module}:ConnectionString` (ex.: `Veterinary:ConnectionString`)

O provider EF Core é definido em `Database:Provider` (`Sqlite` ou `SqlServer`). Development usa SQLite local; Staging/Production esperam SQL Server e secrets via ambiente.

### Variáveis obrigatórias

| Chave / variável de ambiente | Development | Staging / Production | Descrição |
|------------------------------|-------------|----------------------|-----------|
| `ConnectionStrings:DefaultConnection` / `ConnectionStrings__DefaultConnection` | Sim (em `appsettings.Development.json`) | **Obrigatório** (env) | Connection string compartilhada dos módulos |
| `JwtSettings:Secret` / `JwtSettings__Secret` | Sim (dev-only no JSON) | **Obrigatório** (env) | Chave simétrica JWT (mín. 16 caracteres) |
| `JwtSettings:Issuer` | Sim | Sim (JSON ou env) | Emissor do token |
| `JwtSettings:Audience` | Sim | Sim (JSON ou env) | Audiência do token |
| `TenancySettings:DefaultSchema` | Sim (default `dbo`) | Sim | Schema fallback (ADR-003) |
| `Database:Provider` | `Sqlite` | `SqlServer` | Provider EF Core |
| `Database:ConnectionStringName` | `DefaultConnection` | `DefaultConnection` | Nome da entrada em `ConnectionStrings` |

Segredos **não** devem ser commitados. Em Development, use User Secrets quando preferir não manter o JWT no disco:

```bash
dotnet user-secrets set "JwtSettings:Secret" "your-local-secret-min-16-chars" --project src/API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=sysvet.db" --project src/API
```

Containers ( [`src/API/Dockerfile`](../../src/API/Dockerfile) ) recebem as mesmas chaves como variáveis de ambiente no orquestrador.

### Options por módulo

| Seção | Tipo | Projeto |
|-------|------|---------|
| `JwtSettings` | `JwtSettings` | Core.Infrastructure |
| `TenancySettings` | `TenancySettings` | Core.Infrastructure |
| `Database` | `DatabaseOptions` | Core.Infrastructure |
| `Veterinary` | `VeterinaryOptions` | Veterinary.Infrastructure |
| `Inventory` | `InventoryOptions` | Inventory.Infrastructure |
| `Sales` | `SalesOptions` | Sales.Infrastructure |

Petshop e Fiscal ainda não possuem persistência; não registram options de banco.

### Configuração mínima para subir localmente

```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project src/API
```

Com `appsettings.Development.json` versionado, não é obrigatório configurar User Secrets para o primeiro run.

---

## 2. Rastreamento (Trace e Correlation ID)

Foi introduzido o `CorrelationIdMiddleware` para criar um rastro unificado (Trace) das requisições.

- Quando uma requisição entra, buscamos o header `X-Correlation-Id`.
- Caso não exista, um ID único de `Activity.Current` ou `TraceIdentifier` é injetado.
- Este ID é retornado no header de resposta e anexado ao log da aplicação através do `ILogger.BeginScope`. Isso nos permite filtrar nos agregadores de log (como Datadog ou Kibana) todos os logs associados a uma única requisição.

## 3. Health Checks

Os Health Checks foram separados em duas categorias seguindo os padrões do Kubernetes:

- **/health/live (Liveness Probe)**: Retorna apenas se a aplicação web subiu e está respondendo tráfego (ignorando os bancos de dados). Ajuda o orquestrador a saber se a API "morreu" (necessitando de restart).
- **/health/ready (Readiness Probe)**: Avalia a saúde de todas as dependências injetadas (Banco de dados, Serviços Externos, Message Brokers, etc.). Ajuda o Load Balancer a decidir se a API está pronta para *receber* requisições de clientes sem gerar 500.
