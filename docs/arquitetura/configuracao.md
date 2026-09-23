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

Todos os DbContexts de módulo (Core, Veterinary, Inventory, Sales, Petshop, Finance) resolvem a connection string via [`ModuleConnectionStringResolver`](../../src/Modules/Core/Infrastructure/Configuration/ModuleConnectionStringResolver.cs):

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
| *(Rotação JWT)* | — | Runbook | Troca de `JwtSettings:Secret` invalida access tokens até novo login; refresh continua amarrado ao `SecurityStamp` (ADR-007) |
| `TenancySettings:DefaultSchema` | Sim (default `dbo`) | Sim | Schema fallback (ADR-003) |
| `TenancySettings:SingleTenantId` | Não | Dev/staging | Tenant fixo para workers (lembretes/outbox) até varredura multi-tenant (9.x) |
| `Database:Provider` | `Sqlite` | `SqlServer` | Provider EF Core |
| `Database:ConnectionStringName` | `DefaultConnection` | `DefaultConnection` | Nome da entrada em `ConnectionStrings` |
| `Cache:Provider` | `Memory` | `Redis` (staging/prod) | Cache distribuído para queries e entitlements (ADR-057) |
| `Cache:ConnectionString` / `Cache__ConnectionString` | — | **Obrigatório** com Redis | Connection string StackExchange.Redis |
| `Observability:TraceSampleRatio` | `1.0` | `0.1` (typ.) | Amostragem OpenTelemetry (0–1) |
| `Observability:OtlpEndpoint` | — | URL collector | Export OTLP; vazio desliga export |
| `Observability:ConsoleExporter` | `false` | `true` em dev opcional | Trace/metric console |
| `Backup:RetentionDays` | `14` | `14` | Retenção de arquivos `.bak` no operador |

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
| `Cache` | `CacheOptions` | Core.Infrastructure |
| `TenancySettings` | `TenancySettings` | Core.Infrastructure |
| `Database` | `DatabaseOptions` | Core.Infrastructure |
| `Veterinary` | `VeterinaryOptions` | Veterinary.Infrastructure |
| `Inventory` | `InventoryOptions` | Inventory.Infrastructure |
| `Sales` | `SalesOptions` | Sales.Infrastructure |
| `Petshop` | `PetshopOptions` | Petshop.Infrastructure |
| `Finance` | `FinanceOptions` | Finance.Infrastructure |
| `Fiscal` | `FiscalOptions` | Fiscal.Infrastructure |
| `BlobStorage` | `BlobStorageOptions` | Core.Infrastructure |
| `ClinicSite` | `ClinicSiteOptions` | ClinicSite.Infrastructure |
| `Platform` | `PlatformOptions` | Platform.Infrastructure |
| `Platform:Billing` | `BillingOptions` | Platform.Infrastructure |

### Platform — billing SaaS (ADR-049)

| Chave | Default | Descrição |
|-------|---------|-----------|
| `Platform:Billing:Provider` | `Fake` | `Fake` (CI/dev) ou `Asaas` |
| `Platform:Billing:BaseUrl` | `https://api.asaas.com` | Base URL da API Asaas |
| `Platform:Billing:ApiKey` | — | Chave Asaas (**user-secrets/env**, nunca no git) |
| `Platform:Billing:WebhookAccessToken` | — | Token esperado no header `asaas-access-token` |

### Platform — NFS-e SaaS e impersonation (ADR-051)

| Chave | Default | Descrição |
|-------|---------|-----------|
| `Platform:Nfse:Provider` | `Fake` | `Fake` (CI/dev) ou `OpenAc` |
| `Platform:Nfse:IssuerCnpj` | — | CNPJ VetNexus (OpenAc) |
| `Platform:Nfse:IssuerIbgeCityCode` | `0` | IBGE município emissor |
| `Platform:Nfse:CertificateBlobKey` | — | PFX A1 em blob (**user-secrets/env**) |
| `Platform:Nfse:CertificatePassword` | — | Senha do certificado |
| `Platform:Nfse:Environment` | `Homologation` | `Homologation` ou `Production` |
| `Platform:Impersonation:SessionMinutes` | `15` | TTL do JWT de impersonation |

### Platform — resolução de tenant (ADR-046)

| Header | Uso |
|--------|-----|
| `X-Tenant-Id` | GUID do tenant (requests anônimos ou integrações) |
| `X-Tenant-Slug` | Slug resolvido via catálogo `PlatformTenants` |

Rotas isentas de tenant obrigatório: login/refresh/register, `/api/v1/public/*`, health, hubs (ver `TenantEndpointAllowlist` na API).

### Segurança HTTP (Fase 10.6)

| Item | Descrição |
|------|-----------|
| Headers | `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`, `Permissions-Policy` via `SecurityHeadersMiddleware` |
| HSTS | Habilitado fora de `Development` |
| Rate limit | Policy `auth` — 20 req/min por IP em `POST /api/v1/auth/login` e `POST /api/v1/tutor-portal/login|register` |

### Fiscal (NF-e / NFS-e Nacional — ADR-035)

| Chave | Default | Descrição |
|-------|---------|-----------|
| `Fiscal:Provider` | `Fake` | `Fake` (CI/dev) ou `ZeusOpenAc` (Zeus NF-e + OpenAC NFS-e ADN) |
| `Fiscal:CertificateEncryptionKey` | — | AES para senha do PFX (mín. 32 caracteres; **user-secrets/env**, nunca no git) |
| `Fiscal:CertificateEncryptionKeyPrevious` | — | Chave anterior durante rotação (opcional; decrypt de ciphertext legado e `v1:`) |
| `Fiscal:ConnectionString` | *(opcional)* | Override de connection string do módulo |

Exemplo em [`appsettings.Development.json`](../../src/API/appsettings.Development.json).

### Sales — terminal de pagamento (ADR-027)

| Chave | Default | Descrição |
|-------|---------|-----------|
| `Sales:PaymentTerminalProvider` | `Simulator` | Adapter TEF ativo (`Simulator` até integrar acquirer real) |

### ClinicSite (site público — ADR-044)

| Chave | Default | Descrição |
|-------|---------|-----------|
| `ClinicSite:BaseDomain` | `vetnexus.app` | Domínio base para subdomínios `{slug}.vetnexus.app` |

CORS: origens explícitas em `Cors:AllowedOrigins` (inclui `ClinicSiteWeb` em Development) e `SetIsOriginAllowed` para hosts `*.vetnexus.app`.

### BlobStorage (anexos clínicos — ADR-015)

| Chave | Default (Development) | Descrição |
|-------|------------------------|-----------|
| `BlobStorage:Provider` | `Local` | `Local`, `InMemory` (testes) ou `Azure` |
| `BlobStorage:LocalRootPath` | `App_Data/blobs` | Pasta no host quando `Provider=Local` |
| `BlobStorage:AzureConnectionString` | — | Obrigatório se `Provider=Azure` |
| `BlobStorage:AzureContainer` | `clinical` | Container para blobs clínicos |

Exemplo em [`appsettings.Development.json`](../../src/API/appsettings.Development.json). Validação via `ValidateOnStart` na subida da API.

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

### Grooming — SignalR (ADR-031)

| Item | Valor | Descrição |
|------|-------|-----------|
| Hub path | `/hubs/grooming-status` | Broadcast de `GroomingStatusChanged` por tenant |
| Grupo | `tenant-{TenantId}` | Claim `TenantId` do JWT |
| WebSocket auth | Query `access_token` | Mesmo JWT Bearer; ver [`JwtBearerOptionsConfiguration`](../../src/Modules/Core/Infrastructure/Identity/JwtBearerOptionsConfiguration.cs) |
| Permissão hub | `Grooming.Read` | Conexão do backoffice |
| Canal tutor | `ITutorNotificationChannel` | `EnqueueingTutorNotificationChannel` quando Automations registrado (ADR-038) |
| Automations worker | `Automations:PollIntervalSeconds` | Outbox SQL + `OutboxProcessor` (8.1) |
| Lembretes 8.2 | `Automations:Provider` | `Fake` (CI) ou `Live` (SMTP + Evolution — ADR-039) |
| Scan lembretes | `Automations:ReminderScanIntervalMinutes` | `ReminderScheduler` |
| Horário comercial | `Automations:BusinessHours` / DB `AutomationsSettings` | Adia jobs via `MessageJob.DeferUntil` |
| SMTP / Evolution | `Automations:Smtp`, `Automations:Evolution` | Segredos via User Secrets / env |
| Campanhas / NPS 8.3 | `Automations:CampaignScanIntervalMinutes` | `CampaignScheduler` (NPS pós-atendimento) |
| Link NPS | `Automations:PublicBaseUrl` | Base do PWA para `SurveyUrl` |
| Token NPS | `Automations:NpsTokenSigningKey`, `NpsInviteExpiryDays` | API pública `/api/v1/public/nps` |

Clientes Blazor/MAUI usam `IGroomingStatusRealtime` com o client HTTP `API` como base URL.

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

- **Build Linux/WSL:** workload `wasm-tools` + pacote `libatomic1` (link nativo WASM/SQLite via `WasmBuildNative`; sem `libatomic.so.1` o `emcc` falha com exit 127).
- **API base URL:** `src/Clients/BlazorWeb/wwwroot/appsettings.Development.json` → `"ApiBaseUrl": "https://localhost:7180/"`.
- **CORS:** `Cors:AllowedOrigins` na API inclui `https://localhost:7252`, `http://localhost:5259` (BlazorWeb staff), `https://localhost:7262`, `http://localhost:5260` (TutorPortalWeb) e `https://localhost:7282`, `http://localhost:5280` (PlatformWeb Super Admin).

### PlatformWeb Super Admin (Fase 9.8)

- **Client:** `src/Clients/PlatformWeb/` — PWA fino, tokens `platform_*` em localStorage.
- **API base URL:** `src/Clients/PlatformWeb/wwwroot/appsettings.Development.json` → `"ApiBaseUrl": "https://localhost:7180/"`.
- **Dev login:** usuário seed `superadmin@vetnexus.app` / `Password123!` (Development).
- **Run:** `dotnet run --project src/Clients/PlatformWeb/PlatformWeb.csproj` → `https://localhost:7282`.
- Ver [ADR-053](./ADR-053-platform-super-admin-ui.md).

### TutorPortal Web Push (Fase 8.5)

Opcional. Sem chaves VAPID, `ITutorPushSender` usa implementação Fake (subscribe persiste; entrega não ocorre).

| Chave | Obrigatório | Descrição |
|-------|-------------|-----------|
| `TutorPortal:VapidPublicKey` | Para push live | Chave pública URL-safe base64 |
| `TutorPortal:VapidPrivateKey` | Para push live | Chave privada VAPID |
| `TutorPortal:VapidSubject` | Para push live | `mailto:` ou `https:` exigido pelo protocolo |

Tutor autenticado obtém a chave pública via `GET /api/v1/tutor-portal/push/vapid-public-key` e registra o browser em `POST /api/v1/tutor-portal/push/subscribe`. Ver [ADR-042](./ADR-042-tutor-portal-app.md).
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

### OpenTelemetry e alertas operacionais (10.5 — ADR-058)

Composição: [`AddSysVetOpenTelemetry`](../../src/API/Extensions/OpenTelemetryServiceCollectionExtensions.cs) + [`AddOperationalAlerts`](../../src/API/Extensions/OperationalAlertServiceCollectionExtensions.cs).

| Sinal | Origem | Log / métrica |
|-------|--------|----------------|
| `Http5xx` | [`Http5xxOperationalAlertMiddleware`](../../src/API/Middlewares/Http5xxOperationalAlertMiddleware.cs) | Scope `OperationalAlert`; counter `operational.alerts.raised` |
| `SyncPushFailure` | [`PushSyncBatchCommandHandler`](../../src/Modules/Core/Application/Sync/PushSyncBatchCommandHandler.cs) via `ISyncPushObserver` | Janela 1 min (default 5 falhas) |
| `BillingChargeFailure` | [`BillingPaymentExecutor`](../../src/Modules/Platform/Application/Billing/BillingPaymentExecutor.cs) via `IBillingChargeObserver` | Evento imediato por cobrança falha |

Checks **ops** (tag `ops`, apenas em `GET /health`, não em `ready`):

| Nome | Descrição |
|------|-----------|
| `ops-http-5xx` | Degraded se taxa 5xx &gt; limiar |
| `ops-sync-push` | Degraded se falhas push &gt; limiar |
| `ops-billing-failures` | Degraded se faturas `Failed` ≥ `Observability:BillingFailedInvoiceThreshold` |

Backup SQL Server: planner [`SqlServerBackupPlan`](../../src/Modules/Core/Application/Operations/SqlServerBackupPlan.cs), operação [`scripts/sqlserver-backup-restore-drill.sh`](../../scripts/sqlserver-backup-restore-drill.sh), runbook [`backup-dr-runbook.md`](./backup-dr-runbook.md).

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
