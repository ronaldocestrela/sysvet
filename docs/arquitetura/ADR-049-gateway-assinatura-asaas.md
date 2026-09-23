# ADR-049: Gateway de assinatura e cobrança Asaas (9.4)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 9.3 entrega catálogo, assinatura, pró-rata em `SubscriptionAdjustment` (`PendingBilling`) e trial (ADR-048). O backoffice §3 exige gateway para assinatura recorrente do SaaS, cartão/Pix/boleto, webhooks e histórico de faturas — sem dunning, bloqueio operacional, cupons ou NFS-e (9.5–9.6) e sem UI Super Admin (9.8).

## Opções consideradas

1. **Stripe Billing** — recorrência madura; Pix/boleto Brasil menos alinhados ao fluxo local.
2. **Pagar.me** — equivalente brasileiro; menos documentação interna no ecossistema do projeto.
3. **Asaas** — assinaturas BRL, cartão tokenizado, Pix e boleto, webhooks estáveis; escolhido para Live.

Recorrência nativa do Asaas não é usada: pró-rata e `CreditBalance` permanecem no domínio Platform (9.3).

## Decisão

1. **Porta** `IBillingGateway` na Application; `Fake` (CI/dev) e `Asaas` (`HttpClient` v3).
2. **Config** `Platform:Billing:Provider`, `ApiKey`, `BaseUrl`, `WebhookAccessToken` (user-secrets/env).
3. **Persistência** `dbo`: `BillingCustomer`, `BillingPaymentMethod`, `BillingInvoice`, `BillingCharge`, `BillingWebhookReceipt`; `TenantSubscription.BillingStanding` (`Unbilled`, `Good`, `PastDue`, `Canceled`).
4. **Ciclo:** `BillingCycleHostedService` + `ChargeDueSubscriptionsCommand`; fatura = plano + add-ons ativos + ajustes `PendingBilling` positivos; total zero liquida localmente.
5. **Webhook** `POST /api/v1/platform/webhooks/asaas`, header `asaas-access-token`, idempotência por `event + paymentId`.
6. **Eventos:** `PAYMENT_RECEIVED` / `PAYMENT_CONFIRMED` liquidam; `PAYMENT_OVERDUE` → `PastDue`; `PAYMENT_DELETED` cancela cobrança aberta; `PAYMENT_REFUNDED` registra estorno. `TenantStatus` não muda (9.5).

## Consequências

- Aceite: assinatura recorrente cobrada; webhook atualiza `BillingStanding`.
- Cartão: apenas `creditCardToken` Asaas; sem PAN/CVV persistido.
- Financeiro tenant (ADR-027) não participa.

## Confirmação no código

- Domínio: `BillingInvoiceComposer`, `TenantSubscription` billing methods
- Infra: `FakeBillingGateway`, `AsaasBillingGateway`, migration `AddPlatformBilling`
- API: rotas `/api/v1/platform/tenants/{id}/billing/*` e webhook Asaas

## Relacionados

- [ADR-048](./ADR-048-planos-addons-feature-flags.md), [ADR-047](./ADR-047-onboarding-tenants-filiais.md)
- [roadmap](../roadmap.md) §9.4, [backoffice](../backoffice.md) §3
