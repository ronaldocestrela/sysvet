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
- **Perfil:** `GET /api/v1/auth/me` — retorna roles, `profileId`, `permissions`, `menus` (contrato SharedUI Fase 3.1).
- **Register (somente Development):** `POST /api/v1/auth/register` — body `{ "email", "password", "role", "tenantId?" }`; role ∈ `ApplicationRoles`.
- **Staff users (Admin):** `GET/POST /api/v1/users`, `PUT /api/v1/users/{userId}`, disable/enable, reset password.
- **Access profiles (Admin):** `GET/POST/PUT/DELETE /api/v1/access-profiles`, catálogo `GET /api/v1/permissions`.
- **Preferências UI:** `GET/PUT /api/v1/me/preferences` — JSON de atalhos, filtros salvos e colunas.
- **Lockout:** 5 tentativas falhas → bloqueio 15 minutos (`IdentityService` + `SignInManager`).
- Refresh tokens são armazenados **apenas como hash** (`UserRefreshTokens`); ver [ADR-007](./ADR-007-jwt-rbac.md).

Exemplo local (API `https` profile — porta **7180**):

```bash
curl -sk -X POST https://localhost:7180/api/v1/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"admin@sysvet.com","password":"Password123!"}'
```

### Blazor WASM (Fase 3.2)

- **API base URL:** `src/Clients/BlazorWeb/wwwroot/appsettings.Development.json` → `"ApiBaseUrl": "https://localhost:7180/"`.
- **CORS:** `Cors:AllowedOrigins` na API inclui `https://localhost:7252` e `http://localhost:5259` (origens do BlazorWeb dev).
- **PWA:** validar instalação/offline após `dotnet publish src/Clients/BlazorWeb/BlazorWeb.csproj` (service worker ativo no output `wwwroot/`). Ver [ADR-012](./ADR-012-blazor-pwa-jwt.md).
- **SQLite local (3.4):** arquivo `sysvet.db` em MEMFS; snapshot em **IndexedDB** (`sqlite-db-storage.js`) após cada `SaveChanges` com alterações. Boot: `RestoreOfflineDatabaseIfExistsAsync` → `MigrateOfflineDatabaseAsync`. Ver [ADR-014](./ADR-014-sqlite-local-clients.md).

### MAUI Blazor Hybrid (Fase 3.3)

- **API base URL:** `src/Clients/MauiApp/appsettings.json` (MauiAsset) ou defaults em [`MauiApiConfiguration`](../../src/Clients/MauiApp/MauiApiConfiguration.cs):
  - Windows: `https://localhost:7180/` (perfil `https` da API).
  - Android emulador: `http://10.0.2.2:5222/` (host loopback → HTTP da API; cleartext no manifest Android).
- **CORS:** não se aplica ao cliente nativo; apenas configure a URL correta por plataforma.
- **Build/publish:** job CI `maui-publish` (`windows-latest`) ou localmente com workload MAUI. Ver [ADR-013](./ADR-013-maui-blazor-hybrid.md).
- **SQLite local (3.4):** `FileSystem.AppDataDirectory/sysvet.db`; `AddClientPersistence` + `MigrateOfflineDatabaseAsync` no startup. Ver [ADR-014](./ADR-014-sqlite-local-clients.md).

### Migrations EF — banco local do client (`OfflineDbContext`)

Independente das migrations do módulo Core na API:

```bash
dotnet tool restore
dotnet ef migrations add <Name> \
  --project src/Clients/Clients.Infrastructure/Clients.Infrastructure.csproj \
  --context OfflineDbContext
```

Design-time: [`OfflineDbContextFactory`](../../src/Clients/Clients.Infrastructure/Persistence/OfflineDbContextFactory.cs).

### Seed de roles (Identity)

No boot da API, [`IdentityDataSeedHostedService`](../../src/Modules/Core/Infrastructure/Persistence/Seeding/IdentityDataSeedHostedService.cs) executa [`IdentityDataSeeder`](../../src/Modules/Core/Infrastructure/Persistence/Seeding/IdentityDataSeeder.cs), que cria de forma idempotente as roles `Admin`, `Veterinarian`, `Receptionist` e `Cashier` (`ApplicationRoles`). O seed **não** roda enquanto houver migrations pendentes ou o banco estiver inacessível (testes com `EnsureCreated` permanecem válidos).

Em **Development**, [`DevelopmentAdminUserSeedHostedService`](../../src/Modules/Core/Infrastructure/Persistence/Seeding/DevelopmentAdminUserSeedHostedService.cs) garante o usuário `admin@sysvet.com` / `Password123!` (tenant `11111111-1111-1111-1111-111111111111`, perfil Admin).

### Auditoria (Admin)

- **Consulta:** `GET /api/v1/audit-logs` — paginação e filtros `entityName`, `entityId`, `auditAction` (policy `Admin` + permissão `Audit.Read`).
- **Persistência:** append-only em `AuditLogs`; captura automática no `SaveChanges` para entidades `IAuditable` (CRM e `AccessProfile`).

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

---

## 4. Problem Details (`Result.Failure`)

Falhas de aplicação mapeadas por [`ResultExtensions`](../../src/API/Extensions/ResultExtensions.cs) retornam **RFC 7807** (`application/problem+json`):

| Campo | Conteúdo |
|-------|----------|
| `status` | Derivado de `Error.Code` (`*NotFound` → 404, `*Conflict` → 409, `Unauthorized`/`Forbidden` → 401/403, validação → 400) |
| `detail` | `Error.Message` |
| `errors` | Array `{ "Code", "Message" }` (validação: lista de erros de propriedade) |
| `correlationId` | `HttpContext.TraceIdentifier` (middleware + `AddProblemDetails`) |

Mismatch de ID rota/comando usa `Request.RouteIdMismatch` via [`ApiResultHelpers`](../../src/API/Extensions/ApiResultHelpers.cs).
