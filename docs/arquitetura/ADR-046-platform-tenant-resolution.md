# ADR-046: Platform — resolução de tenant e catálogo global

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 9.1 exige módulo `Platform`, resolução de tenant por JWT/header/host e enforcement em todos os módulos (roadmap §9.1). O ADR-003 já define schema por tenant; faltava catálogo `dbo`, cadeia unificada de resolução e teste de isolamento.

## Decisão

1. Criar `src/Modules/Platform/{Domain,Application,Infrastructure}` com entidade `Tenant` (catálogo global `PlatformTenants`).
2. Substituir `TenantClaimMiddleware` por `TenantResolutionMiddleware` na API, com ordem: **JWT** → **`X-Tenant-Id` / `X-Tenant-Slug`** → **subdomínio** → fallback `TenancySettings`.
3. JWT autenticado é fonte de verdade; header/host conflitantes retornam **403** (`Platform.Tenancy.TenantMismatch`).
4. `TenantRequiredEndpointFilter` exige `TenantId` em `/api/v1/*`, com allowlist (auth login/refresh/register, `/api/v1/public/*`, health, hubs).
5. Helper compartilhado `TenantSchema.FromId` em `Core.Domain`.
6. Defesa em profundidade: `TenantIsolationModelBuilderExtensions` (shadow `TenantId` + query filter) nos DbContexts tenant-scoped; índices globais (`ClinicSiteSlugIndex`, `MarketplaceSellerIndex`) excluídos.
7. `ITenantDirectory` / `ITenantSlugLookup` no Platform; `OutboxProcessor` itera tenants do catálogo com fallback single-scope.

## Consequências

- Onboarding completo, status de tenant e Super Admin permanecem na 9.2+.
- SQLite/CI dependem de filtros por `TenantId` além de schema (ignorado pelo provider SQLite).

## Confirmação no código

- [`TenantResolutionMiddleware`](../../src/API/Middlewares/TenantResolutionMiddleware.cs)
- [`TenantRequiredEndpointFilter`](../../src/API/Filters/TenantRequiredEndpointFilter.cs)
- [`PlatformDbContext`](../../src/Modules/Platform/Infrastructure/Persistence/PlatformDbContext.cs)
- [`TenantIsolationEndpointsTests`](../../tests/API.IntegrationTests/Platform/TenantIsolationEndpointsTests.cs) — `TenantIsolation_DoesNotLeakCrm_WhenDifferentTenants`

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-038](./ADR-038-automations-outbox.md)
- [roadmap](../roadmap.md) §9.1
