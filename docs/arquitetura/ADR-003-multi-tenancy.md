# ADR 003: Multi-tenancy (schema por tenant)

## Status
Accepted

## Data
2026-08-20

## Contexto

O SaaS atende múltiplas clínicas (tenants). Dados de um tenant não podem vazar para outro. A solução usa EF Core com SQL Server em produção e SQLite em desenvolvimento/CI, com JWT identificando tenant e usuário.

**Drivers:** isolamento forte; compatibilidade com monólito modular (ADR-001); provisionamento futuro via módulo `Platform` (Fase 9).

## Opções consideradas

1. **Discriminator (`TenantId` + filtro global)** — uma tabela física, coluna `TenantId`, query filter no EF.
   - Prós: migrations simples; um schema.
   - Contras: risco de vazamento se filtro falhar; índices e volume únicos por tenant mais trabalhosos.

2. **Schema separado por tenant** — mesmo banco SQL Server; `modelBuilder.HasDefaultSchema(tenantSchema)` por request.
   - Prós: isolamento lógico forte; consultas sem `TenantId` em toda linha; backup/restore por schema possível.
   - Contras: migrations aplicadas em N schemas; cache de modelo EF deve variar por schema.

3. **Database por tenant** — instância ou catalog separado.
   - Prós: isolamento máximo.
   - Contras: custo operacional e provisionamento; fora do escopo do MVP.

## Decisão

Usar **schema SQL separado por tenant**, resolvido em runtime via `ITenantContext.SchemaName`, preenchido após autenticação (`TenantClaimMiddleware`). Fallback configurável `TenancySettings:DefaultSchema` (ex. `dbo`) para desenvolvimento e testes sem claim.

Provisionamento em massa de schemas, onboarding Super Admin e impersonation auditada ficam no módulo **Platform** (roadmap Fase 9), não bloqueiam esta decisão.

## Consequências

- **Positivas:** menor chance de cross-tenant acidental em SQL ad hoc; alinhado a clínicas como unidades isoladas.
- **Negativas:** pipeline de migrations multi-schema; `IModelCacheKeyFactory` customizado obrigatório.
- **Futuro:** testes de isolamento E2E por tenant; automação de `CREATE SCHEMA` no onboarding.

## Confirmação no código

- Contrato: [`ITenantContext`](../../src/Modules/Core/Domain/ITenantContext.cs) — `TenantId`, `UserId`, `SchemaName`.
- Implementação scoped: [`DefaultTenantContext`](../../src/Modules/Core/Infrastructure/Tenancy/DefaultTenantContext.cs); options [`TenancySettings`](../../src/Modules/Core/Infrastructure/Tenancy/TenancySettings.cs).
- Request pipeline: [`TenantClaimMiddleware`](../../src/Modules/Core/Infrastructure/Identity/TenantClaimMiddleware.cs) após `UseAuthentication()` em [`Program.cs`](../../src/API/Program.cs).
- EF: [`TenantAwareModelCacheKeyFactory`](../../src/Modules/Core/Infrastructure/Persistence/TenantAwareModelCacheKeyFactory.cs) inclui `SchemaName` na chave de modelo.
- DbContexts: `CoreDbContext`, `VeterinaryDbContext`, `InventoryDbContext`, `SalesDbContext` — `HasDefaultSchema` derivado de `ITenantContext` (migrations atuais geradas com schema `dbo` como baseline de design-time).

Documentação de config: [`configuracao.md`](./configuracao.md) (`TenancySettings:DefaultSchema`).

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-002](./ADR-002-estrategia-de-sync.md)
- [configuracao.md](./configuracao.md)
- [roadmap](../roadmap.md) — Fase 9 Platform
