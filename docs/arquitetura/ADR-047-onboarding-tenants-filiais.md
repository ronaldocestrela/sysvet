# ADR-047: Onboarding de tenants, status e filiais (9.2)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 9.1 entrega catálogo `PlatformTenants` e resolução de tenant (ADR-046). O backoffice §1 exige onboarding (tenant + admin + seed), ciclo de vida (`ativo`, `suspenso`, `cancelado`, `excluído`) e filiais multi-CNPJ na mesma conta matriz. A UI Super Admin fica na 9.8; esta fatia expõe API `/api/v1/platform/*` para operadores `SuperAdmin`.

## Decisão

1. **Filiais (`PlatformBranches`)** no schema global `dbo`, vinculadas a `TenantId`. Filial não é tenant nem schema separado. Uma filial matriz (`IsHeadquarters`) por tenant; CNPJ único entre filiais não excluídas do tenant.
2. **Status no agregado `Tenant`:** `Active`, `Suspended`, `Cancelled`, `Deleted` (soft delete com `DeletedAt`). Slug único apenas entre tenants não excluídos.
3. **Onboarding síncrono** (`OnboardTenantCommand`): catálogo → provisionamento → seed de perfis → usuário `Admin` via `IIdentityService`. Compensação remove linhas do catálogo se a criação do admin falhar.
4. **Provisionamento (estado atual das migrations):** tabelas de módulos permanecem em `schema: dbo` com isolamento por `TenantId` (filtros EF). Em SQL Server, `CREATE SCHEMA tenant_{guid}` é executado no onboard (preparação ADR-003); clonagem de tabelas por schema fica fora do escopo até reescrita de migrations.
5. **Enforcement:** login e rotas operacionais `/api/v1/*` (exceto allowlist) exigem tenant `Active`. `ITenantDirectory` lista só tenants `Active` para workers.
6. **Autorização:** role `SuperAdmin` (`TenantId` vazio), policy `PlatformAdmin`.

## Consequências

- Health check, impersonation e billing permanecem em 9.5–9.7.
- `IssuerProfile` e transferência entre filiais (ADR-020) não são alterados nesta fatia.

## Confirmação no código

- Domínio: `Platform.Domain.Entities.Tenant`, `Branch`, `TenantStatus`
- API: `PlatformEndpointsExtensions`
- Gate: `ITenantSignInGate` (Core.Application), `CatalogTenantSignInGate` (Platform.Infrastructure)

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-046](./ADR-046-platform-tenant-resolution.md)
- [roadmap](../roadmap.md) §9.2, [backoffice](../backoffice.md) §1
