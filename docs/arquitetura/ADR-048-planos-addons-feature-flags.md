# ADR-048: Planos, add-ons e feature flags (9.3)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 9.2 entrega catálogo de tenants e onboarding (ADR-047). O backoffice §2 exige planos comerciais, add-ons, flags por tenant, pró-rata e trial — sem gateway de pagamento (9.4) e sem UI Super Admin (9.8).

## Decisão

1. **Catálogo global** em `PlatformDbContext` (`dbo`): `Plan`, `AddOn`, `PlanIncludedModule`, `TenantSubscription`, `TenantAddOn`, `FeatureFlag`, `SubscriptionAdjustment`.
2. **`CommercialModule`** em `Core.Domain.Entitlements` — enum compartilhado entre API, menus e Platform.
3. **Efetivo:** `(plan ∪ add-ons − flags Disabled) ∪ flags Enabled`; `TrialExpired` zera módulos comerciais (núcleo CRM/usuários permanece).
4. **Seed:** Starter / Pro / Hospital24h e add-ons Estética, Fiscal, Automação, PDV Offline; tenants existentes sem assinatura recebem grandfather (Hospital24h + add-ons); onboarding novo default Starter.
5. **Pró-rata:** período 30 dias; ajustes em `SubscriptionAdjustment` com `PendingBilling` (cobrança na 9.4).
6. **Trial:** `ExpireDueTrialsCommand` + `TrialExpirationHostedService`; ações `Block` (→ `TrialExpired`) ou `Convert` (→ `Active`).
7. **Cache:** `ITenantEntitlementReader` + `IMemoryCache` (TTL 5 min, invalidação em mutações); fallback `AllowAllEntitlementReader` no Core quando Platform não registra implementação.
8. **Enforcement:** `CommercialModuleEndpointFilter` + `CommercialModuleRouteMapper`; menus filtrados em `GetCurrentUserQueryHandler` via `MenuEntitlementMapper`.
9. **API Super Admin:** `/api/v1/platform/plans`, `/addons`, assinatura, change plan, add-ons, flags e entitlements — policy `PlatformAdmin`.

## Consequências

- Workers/outbox não consultam entitlement nesta fatia (HTTP + `/auth/me` apenas).
- UI de gestão comercial permanece na 9.8; operação via API Scalar.

## Confirmação no código

- Domínio: `Platform.Domain.Services.EntitlementCalculator`, `ProrationCalculator`
- Infra: `TenantEntitlementReader`, `TenantSubscriptionProvisioner`, migration `AddCommercialCatalog`
- API: `CommercialModuleEndpointFilter`, `PlatformEndpointsExtensions`
- Testes: `FeatureFlag_DisablesModule_OnApiAndMenus`, `PlanChange_ReturnsProration_WhenMidCycle`

## Relacionados

- [ADR-047](./ADR-047-onboarding-tenants-filiais.md), [ADR-046](./ADR-046-platform-tenant-resolution.md)
- [roadmap](../roadmap.md) §9.3, [backoffice](../backoffice.md) §2
