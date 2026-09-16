# `src/Modules/Core/Infrastructure/` — Camada de Infraestrutura do Módulo Core

Camada responsável pela **implementação técnica** das abastrações definidas nas camadas de `Domain` e `Application`. Conecta o domínio ao mundo real: banco de dados, ORMs, Identity, JWT e serviços transversais.

## Status Atual

> **Implementado (Fase 2.2).** `CoreDbContext`, mapeamentos EF Core, repositórios genéricos, UoW, migrations (`InitialCore`), seed de roles Identity, [`DependencyInjection.AddCoreModule`](./DependencyInjection.cs).

## Estrutura

```
Infrastructure/
├── Configuration/              # DatabaseOptions, ConfigureModuleDatabase, Options tipados
├── HealthChecks/               # DatabaseHealthCheck (core-db)
├── Identity/                   # AppUser, JWT, TenantClaimMiddleware
├── Persistence/
│   ├── CoreDbContext.cs
│   ├── CoreDbContextFactory.cs # Design-time EF CLI (ADR-003 baseline dbo)
│   ├── TenantAwareModelCacheKeyFactory.cs
│   ├── Configurations/         # Tutor, Pet, AuditLog, IdempotencyRecord
│   ├── Migrations/             # InitialCore (Identity + CRM + audit/idempotency)
│   ├── Repositories/           # Repository<T>, TutorRepository, PetRepository
│   └── Seeding/                # IdentityDataSeeder + hosted service
├── Tenancy/                    # DefaultTenantContext, TenancySettings
├── Auditing/
├── Services/
└── DependencyInjection.cs
```

## Regras desta Camada

- ✅ Implementa interfaces de repositório definidas em `Domain`
- ✅ O `CoreDbContext` contém apenas tabelas do módulo Core (mais Identity compartilhado)
- ✅ **Schema lógico do módulo** (boundary ADR-001) ≠ **schema SQL do tenant** (ADR-003: `dbo` / `tenant_{guid}`)
- ✅ Providers: **SQL Server** (Staging/Production) e **SQLite** (Development), via `Database:Provider`
- ❌ Nenhuma lógica de negócio — apenas persistência, Identity e integrações técnicas
- ❌ Nunca referenciada diretamente pela camada `Application`

## Migrations (Core)

```bash
dotnet tool restore
dotnet ef database update \
  --project src/Modules/Core/Infrastructure/Core.Infrastructure.csproj \
  --startup-project src/API/API.csproj \
  --context CoreDbContext
```

Roles (`Admin`, `Veterinarian`, `Receptionist`, `Cashier`) são criadas no boot via `IdentityDataSeedHostedService` quando não há migrations pendentes.
