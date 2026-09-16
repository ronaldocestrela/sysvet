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
| `JwtSettings:RefreshExpiryDays` | Sim (default 7) | Sim | Validade do refresh token (dias) |
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

### Migrations EF Core (módulo Core)

Ferramenta local (manifesto na raiz):

```bash
dotnet tool restore
dotnet ef database update \
  --project src/Modules/Core/Infrastructure/Core.Infrastructure.csproj \
  --startup-project src/API/API.csproj \
  --context CoreDbContext
```

Design-time: [`CoreDbContextFactory`](../../src/Modules/Core/Infrastructure/Persistence/CoreDbContextFactory.cs) usa SQLite e schema `dbo` (baseline ADR-003). Em Development, o banco padrão é `sysvet.db` (`ConnectionStrings:DefaultConnection`).

### Autenticação JWT (Fase 2.3)

- **Login:** `POST /api/v1/auth/login` — body `{ "email", "password" }` → `{ accessToken, refreshToken, expiresInSeconds }`.
- **Refresh:** `POST /api/v1/auth/refresh` — body `{ "refreshToken" }` (rotação; token antigo invalidado).
- **Perfil:** `GET /api/v1/auth/me` — header `Authorization: Bearer {accessToken}`.
- **Register (somente Development):** `POST /api/v1/auth/register` — body `{ "email", "password", "role", "tenantId?" }`; role ∈ `ApplicationRoles`.
- **Lockout:** 5 tentativas falhas → bloqueio 15 minutos (`IdentityService` + `SignInManager`).
- Refresh tokens são armazenados **apenas como hash** (`UserRefreshTokens`); ver [ADR-007](./ADR-007-jwt-rbac.md).

Exemplo local:

```bash
curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@sysvet.com","password":"Password123!"}'
```

### Seed de roles (Identity)

No boot da API, [`IdentityDataSeedHostedService`](../../src/Modules/Core/Infrastructure/Persistence/Seeding/IdentityDataSeedHostedService.cs) executa [`IdentityDataSeeder`](../../src/Modules/Core/Infrastructure/Persistence/Seeding/IdentityDataSeeder.cs), que cria de forma idempotente as roles `Admin`, `Veterinarian`, `Receptionist` e `Cashier` (`ApplicationRoles`). O seed **não** roda enquanto houver migrations pendentes ou o banco estiver inacessível (testes com `EnsureCreated` permanecem válidos).

---

## 2. Rastreamento (Trace e Correlation ID)

O [`CorrelationIdMiddleware`](../../src/API/Middlewares/CorrelationIdMiddleware.cs) cria um rastro unificado por requisição HTTP.

- Entrada: lê o header `X-Correlation-Id`; se ausente, usa `Activity.Current?.Id` ou `HttpContext.TraceIdentifier`.
- O valor é fixado em `HttpContext.TraceIdentifier`, devolvido no header de resposta e incluído no escopo de log (`CorrelationId`) via `ILogger.BeginScope`.
- **Logging estruturado:** em [`appsettings.json`](../../src/API/appsettings.json), o console usa formatter **JSON** com `IncludeScopes: true`, para que agregadores (Datadog, Kibana, etc.) indexem o correlation id sem Serilog.

## 3. Health Checks

Registro e mapeamento: [`AddCoreModule`](../../src/Modules/Core/Infrastructure/DependencyInjection.cs) (`core-db`) + [`AddApiHealthChecks` / `MapApiHealthChecks`](../../src/API/Extensions/HealthCheckExtensions.cs) (`api`). Rotas são **anônimas** (sem JWT).

| Rota | Tags | Comportamento |
|------|------|----------------|
| `GET /health/live` | `live` | Liveness: apenas o processo (`api`). **Não** consulta banco. Resposta texto `Healthy` / `Unhealthy`. |
| `GET /health/ready` | `ready` | Readiness: processo + `core-db` (`CanConnectAsync` no `CoreDbContext`). HTTP 503 se alguma dependência falhar. |
| `GET /health` | (todos) | Status **agregado em JSON** (`application/json`) para orquestradores e dashboards. |

Exemplo de corpo em `GET /health` (200):

```json
{
  "status": "Healthy",
  "duration": "00:00:00.0123456",
  "checks": [
    { "name": "api", "status": "Healthy", "duration": "...", "description": "API process is running." },
    { "name": "core-db", "status": "Healthy", "duration": "...", "description": "Core database connection succeeded." }
  ]
}
```

O check `core-db` respeita `Database:Provider` (`Sqlite` em Development, `SqlServer` em Staging/Production); não expõe connection string nos logs.

Exemplo de probes Kubernetes:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 10
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
```
