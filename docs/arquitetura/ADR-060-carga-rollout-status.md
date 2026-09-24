# ADR-060: Carga multi-tenant, rollout e status page (Fase 10.7)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 10.4 ([ADR-057](./ADR-057-cache-paginacao-baseline.md)) entrega baseline P95 sequencial de um tenant no CI e adia carga **N tenants × M usuários** para a 10.7. A 10.5 expõe health agregado e alertas `ops-*` ([ADR-058](./ADR-058-backup-dr-observabilidade.md)). Não há orquestrador de tráfego no repositório; o rollout de release deve ser operacional via catálogo Platform.

## Opções consideradas

1. **Percentual de tráfego canary no ingress** — depende de infra externa; rejeitado nesta fase.
2. **`ReleaseRing` no agregado `Tenant` + checklist operacional** — alinhado a Super Admin e ADR-047; aceito.
3. **Status page HTML separada** — rejeitado; aceito JSON público anônimo consumível por status page externa.
4. **k6 no GitHub Actions contra SQL Server** — flaky e caro; rejeitado; CI mantém harness in-process reduzido.

## Decisão

### Meta de capacidade

| Meta | Valor |
|------|-------|
| Tenants `Active` no mesmo database | **50** |
| Aceite staging | **10 tenants × 5 usuários** (50 VUs), 5 min, rotas críticas da 10.4 |
| SLO staging | P95 &lt; 500 ms, erro HTTP &lt; 1%, zero vazamento cross-tenant |
| Guardrail CI | **3 tenants × 2 requisições concorrentes**, Memory cache + SQLite |

### Rollout

1. Enum `ReleaseRing`: `Canary`, `Beta`, `GeneralAvailability` (default em novos tenants e migration).
2. Super Admin altera anel via `PATCH /api/v1/platform/tenants/{id}/release-ring`; auditoria `ReleaseRingSet`.
3. Sequência operacional: canary (tenant interno) → beta (cohorte marcada) → GA (demais); artefato `sysvet-api-release` do CI.

### Status page e incidentes

1. Agregado `StatusIncident` (título, impacto, componentes, início, resolução).
2. Super Admin: `GET/POST /api/v1/platform/status/incidents`, `PATCH .../{id}/resolve`.
3. Público: `GET /api/v1/public/status` — status agregado, componentes derivados de health checks (`api`, `core-db`, `redis` quando presente, `ops-sync-push`, `ops-billing-failures`), incidentes abertos sem PII.
4. `/health` permanece contrato de balanceador; status público é produto/comunicação.

## Consequências

- Drill de carga completo e rollout documentados em runbooks; aceite igual 10.5/10.6 (evidência manual + CI guardrail).
- Anel de release não bloqueia login; apenas rastreabilidade operacional até existir feature gating por anel.
- Migration `AddPlatformReleaseRingAndStatusIncidents`.

## Confirmação no código

- [`ReleaseRing`](../../src/Modules/Platform/Domain/Entities/ReleaseRing.cs), [`Tenant.SetReleaseRing`](../../src/Modules/Platform/Domain/Entities/Tenant.cs)
- [`StatusIncident`](../../src/Modules/Platform/Domain/Entities/StatusIncident.cs)
- [`MultiTenantConcurrentLoadTests`](../../tests/LoadTests/MultiTenantConcurrentLoadTests.cs)
- [`PlatformPublicEndpointsExtensions`](../../src/API/Extensions/PlatformPublicEndpointsExtensions.cs)

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-047](./ADR-047-onboarding-tenants-filiais.md), [ADR-057](./ADR-057-cache-paginacao-baseline.md), [ADR-058](./ADR-058-backup-dr-observabilidade.md)
- [`load-capacity.md`](./load-capacity.md), [`rollout-runbook.md`](./rollout-runbook.md), [`incident-runbook.md`](./incident-runbook.md)
- [`docs/roadmap.md`](../roadmap.md) §10.7
