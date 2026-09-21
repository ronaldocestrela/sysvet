# `src/Modules/Finance/` — Módulo Financeiro

Módulo responsável pela **gestão financeira** da clínica/petshop: contas a pagar e receber, caixa, conciliação e fluxo de caixa ([`functions.md`](../../../docs/functions.md) § Financeiro).

## Status

> **Fase 7.2 concluída (ADR-032).** AP/AR materializados a partir de vendas e NF-e de compra; projeção previsto vs realizado; caixa TEF/conciliação na **7.3**.

## Escopo entregue (7.2)

- Agregados `FinancialTitle`, `FinancialCategory`, `CostCenter`
- Consumo de `OrderPaidEvent`, `PurchaseInvoiceImportedEvent`, estorno/devolução
- API `/api/v1/financial-*`; permissões `Finance.*`; sync e espelho SQLite nos clients

## Escopo previsto (7.3+)

- Caixa operacional (sangrias, conciliação TEF), DRE/fluxo completo (7.4), fiscal (7.5+)

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
