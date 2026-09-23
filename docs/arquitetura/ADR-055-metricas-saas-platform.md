# ADR-055: Métricas SaaS globais (Fase 10.2)

## Status
Accepted

## Data
2026-09-23

## Contexto

O backoffice §5 exige MRR, ARR, fluxo de caixa, churn, LTV, CAC e relatório de inadimplência reconciliáveis com o billing Platform (9.4–9.5). A UI Super Admin (9.8) cobre operação diária; BI global permanece fora do módulo Intelligence (tenant-side, ADR-054). Não há CRM externo de aquisição no escopo atual.

## Opções consideradas

1. **Novo módulo Intelligence global** — mistura escopo tenant e plataforma; rejeitado.
2. **Queries no Platform + calculadora pura de domínio** — reutiliza `BillingInvoice`, `TenantSubscription`, `Tenant.CancelledAt`; aceito.
3. **Integração HubSpot/RD para CAC** — dependência externa não entregue; rejeitado — `AcquisitionSpend` manual via Super Admin.

## Decisão

1. **`SaasMetricsCalculator`** e **`SaasCivilMonthRange`** (`America/Sao_Paulo`) na Domain Platform; tolerâncias de aceite `MoneyTolerance` 0,01 BRL e `RateTolerance` 0,0001.
2. **MRR faturado** = faturas do mês (`PeriodStart`) em `Open`/`Failed`/`Paid`; **ARR** = MRR × 12; **caixa** por `PaidAt`/`RefundedAt`.
3. **Churn logos** = cancelamentos no mês (`CancelledAt`) ÷ base início (pagantes ativos + cancelados no mês).
4. **LTV** = (MRR ÷ tenants pagantes no mês) ÷ churn; **CAC** = gasto `AcquisitionSpend` ÷ novos pagantes (primeira fatura `Paid` no mês).
5. **`AcquisitionSpend`** (ano/mês/canal) com upsert Super Admin; **`Tenant.CancelledAt`**, **`BillingInvoice.RefundedAt`**.
6. API **`GET /api/v1/platform/metrics`**, **`POST /api/v1/platform/metrics/acquisition-spend`**; UI **PlatformWeb** `/metrics`.

## Consequências

- Aceite: `SaasMetrics_MatchBilling_WithinDocumentedTolerance` (domínio e integração).
- MRR contratado (catálogo) exposto para diagnóstico; aceite oficial usa MRR faturado.
- Heatmap adoção de módulos permanece Fase 10.3; cache Redis Fase 10.4.

## Confirmação no código

- [`SaasMetricsCalculator`](../../src/Modules/Platform/Domain/Services/SaasMetricsCalculator.cs)
- [`GetPlatformSaasMetricsQueryHandler`](../../src/Modules/Platform/Application/Metrics/SaasMetricsHandlers.cs)
- [`PlatformEndpointsExtensions`](../../src/API/Extensions/PlatformEndpointsExtensions.cs)
- [`MetricsPage.razor`](../../src/Clients/PlatformWeb/Pages/MetricsPage.razor)

## Relacionados

- [ADR-049](./ADR-049-gateway-assinatura-asaas.md), [ADR-053](./ADR-053-platform-super-admin-ui.md), [ADR-054](./ADR-054-intelligence-dashboards-tenant.md)
- [`docs/roadmap.md`](../roadmap.md) §10.2, [`docs/backoffice.md`](../backoffice.md) §5
