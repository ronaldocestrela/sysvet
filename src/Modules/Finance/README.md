# `src/Modules/Finance/` — Módulo Financeiro

Módulo responsável pela **gestão financeira** da clínica/petshop: contas a pagar e receber, caixa, conciliação e fluxo de caixa ([`functions.md`](../../../docs/functions.md) § Financeiro).

## Status

> **Fases 7.2–7.3 concluídas (ADR-032, ADR-033).** AP/AR + conciliação PoC de cartão (NSU); caixa operacional permanece no Sales.

## Escopo entregue (7.2)

- Agregados `FinancialTitle`, `FinancialCategory`, `CostCenter`
- Consumo de `OrderPaidEvent`, `PurchaseInvoiceImportedEvent`, estorno/devolução
- API `/api/v1/financial-*`; permissões `Finance.*`; sync e espelho SQLite nos clients

## Escopo entregue (7.3)

- `TitleAllocation.ExternalReference` (NSU do TEF na alocação de cartão)
- `CardReconciliationBatch` — import JSON manual, match `Matched` / `Divergent` / `Unmatched`
- API `/api/v1/finance/card-reconciliations`, `/finance/card-settlements/unmatched`
- Cliente: página `/finance/card-reconciliation` (online-only)

## Escopo previsto (7.4+)

- DRE/fluxo completo (7.4), fiscal (7.5+)

## Estrutura de Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Títulos, categorias, centros de custo, repositórios |
| [`Application/`](./Application/) | CQRS, integração MediatR, projeções |
| [`Infrastructure/`](./Infrastructure/) | `FinanceDbContext`, migrations, sync, `AddFinanceModule` |

## Referências

- [`docs/roadmap.md`](../../../docs/roadmap.md) — Fase 7
- [`docs/arquitetura/ADR-032-contas-pagar-receber.md`](../../../docs/arquitetura/ADR-032-contas-pagar-receber.md)
- [`docs/arquitetura/ADR-025-motor-pdv-vendas.md`](../../../docs/arquitetura/ADR-025-motor-pdv-vendas.md)
- [`docs/arquitetura/ADR-021-entrada-xml-nfe-compra.md`](../../../docs/arquitetura/ADR-021-entrada-xml-nfe-compra.md)
