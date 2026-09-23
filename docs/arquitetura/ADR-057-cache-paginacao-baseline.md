# ADR-057: Cache distribuído, paginação e baseline de carga (Fase 10.4)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 10.4 exige cache Redis para consultas pesadas, paginação com teto em listagens de volume, índices alinhados a KPIs/relatórios e baseline de carga documentado (P95 &lt; 500 ms em staging). ADR-054–056 adiaram cache; listagens retornavam coleções inteiras.

## Opções consideradas

1. **Cache por handler ad hoc** — duplicação e invalidação inconsistente; rejeitado.
2. **`ICacheableQuery` + `DistributedCacheBehavior` no pipeline MediatR + `IDistributedCache` (Memory/Redis)** — alinhado a CQRS e multi-instância; aceito.
3. **Paginar só na API sem contrato comum** — rejeitado; aceito `PageRequest` + `PagedResult<T>` com máximo 100.

## Decisão

1. **`Cache:Provider`** `Memory` (dev/CI) ou `Redis` (staging/prod); entitlements em `TenantEntitlementReader` usam o mesmo store.
2. Queries cacheáveis: dashboard tenant (30 s), ABC/produtividade, métricas SaaS, adoção (5 min); chave `sysvet:cache:{tenant|platform}:{QueryName}:{suffix}`.
3. **`PageRequest`**: página ≥ 1, `pageSize` default 20, máximo 100; erros `Pagination.*`.
4. Listagens paginadas: produtos, títulos financeiros, pedidos commerce, tenants/login/change/impersonation audits, faturas billing; `take` legado (sync, automations, etc.) limitado a 100.
5. Índices: `Orders (TenantId, PaidAt)`; `Appointments` e `GroomingAppointments (TenantId, Status, Date)`.
6. Baseline: projeto `tests/LoadTests` (CI, Memory/SQLite); runbook [`load-baseline.md`](./load-baseline.md) para staging.

## Consequências

- Aceite: P95 &lt; 500 ms nos endpoints críticos do baseline.
- Catálogos pequenos (planos, permissões, serviços de banho) permanecem sem paginação.
- Redis obrigatório em staging/prod via `Cache:ConnectionString`; health check `redis` tagged `ready`.

## Confirmação no código

- [`DistributedCacheBehavior`](../../src/Modules/Core/Application/Behaviors/DistributedCacheBehavior.cs)
- [`DistributedCacheServiceCollectionExtensions`](../../src/Modules/Core/Infrastructure/Configuration/DistributedCacheServiceCollectionExtensions.cs)
- [`PageRequest`](../../src/Modules/Core/Application/Common/PageRequest.cs)
- [`CriticalEndpointsLoadBaselineTests`](../../tests/LoadTests/CriticalEndpointsLoadBaselineTests.cs)

## Relacionados

- [ADR-054](./ADR-054-intelligence-dashboards-tenant.md), [ADR-055](./ADR-055-metricas-saas-platform.md), [ADR-056](./ADR-056-curva-abc-produtividade-adocao.md)
- [`docs/roadmap.md`](../roadmap.md) §10.4
