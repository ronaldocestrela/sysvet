# ADR-050: Dunning, bloqueio operacional e cupons (9.5)

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 9.4 entrega gateway Asaas/Fake, faturas, webhooks e `BillingStanding` (`PastDue` sem bloqueio operacional — ADR-049). O backoffice §3 exige régua e-mail/SMS, retentativa de cartão, bloqueio após X dias (apenas pagamento) e cupons promocionais.

## Opções consideradas

1. **Reutilizar `Tenant.Suspend()`** — bloqueia login e impede tela de pagamento; rejeitado.
2. **Enfileirar avisos no módulo Automations** — escopo tutor/clínica; SMS recusado (ADR-039); rejeitado.
3. **Bloqueio via `BillingStanding.Locked` + filtro API** — login mantido; rotas operacionais 403; allowlist `/api/v1/billing/*` para admin do tenant; escolhido.

## Decisão

1. **`BillingStanding.Locked`** distinto de `TenantStatus`; `PastDueSince` na assinatura; lock após `Platform:Dunning:LockAfterDays` (padrão 7).
2. **Régua** `DunningScheduleCalculator` (dias 0, 3, 7; e-mail e SMS); entidade `DunningNotice` idempotente; `IDunningNotifier` Fake/Live (SMTP no Platform; SMS HTTP opcional).
3. **Retentativa cartão** contador na fatura (`CardRetryCount`); Pix/boleto sem nova cobrança automática.
4. **Cupons** `Coupon` + `CouponRedemption` (único por tenant+cupom); desconto em `BillingInvoiceComposer`; resgate via Super Admin API.
5. **Clínica** `GET/POST /api/v1/billing/*` (Admin); filtro `TenantOperationalBillingEndpointFilter`; SharedUI `/billing/payment`.
6. **`ProcessDunningCommand`** + `DunningHostedService`.

## Consequências

- Aceite: inadimplência simulada suspende acesso operacional.
- NFS-e SaaS e UI Super Admin permanecem 9.6 e 9.8.

## Confirmação no código

- Domínio: `TenantSubscription` lock, `Coupon`, `DunningScheduleCalculator`
- Infra: migration dunning/cupons, `DunningHostedService`, notifiers
- API: filtro operacional, endpoints billing clínica e cupons platform

## Relacionados

- [ADR-049](./ADR-049-gateway-assinatura-asaas.md), [ADR-048](./ADR-048-planos-addons-feature-flags.md)
- [roadmap](../roadmap.md) §9.5, [backoffice](../backoffice.md) §3
