# ADR-020: Movimentações lot-aware, transferência e alertas de estoque

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 5.2 exige movimentações de entrada/saída/ajuste, transferência entre lotes, kardex e alertas de estoque mínimo e validade. A 5.1 entregou catálogo e saldo por lote, mas `RegisterStockMovement` atualizava apenas `ProductBalance` legado.

## Decisão
- **Saldo:** movimentações aplicam delta em `ProductLot` quando o produto exige lote ou possui lotes; caso contrário, `ProductBalance.ApplySignedDelta`.
- **Ajuste:** `MovementType.Adjustment` exige `AdjustmentDirection` (Increase/Decrease); quantidade permanece sempre positiva no ledger.
- **Transferência:** par Out/In no mesmo produto, lotes distintos, `CorrelationId` compartilhado; sem depósito/filial (Fase 9.2).
- **Kardex:** query paginada por produto com saldo corrido calculado na Application (não event-sourcing).
- **Alertas:** projeção CQRS (`LowStock`, `ExpiringSoon`, `Expired`) estilo ADR-016; sem worker/push.
- **FEFO:** `LotAllocationService` apenas em `OrderPaidEvent`; saída manual exige lote explícito quando `RequiresLot`.
- **Sync:** pull/push de `StockMovement`; alertas recalculados no cliente.
- **Permissões:** mutações `Stock.Write`; leituras kardex/alertas/lista `Stock.Read`.

## Consequências
- `StockMovement` ganha `AdjustmentDirection`, `CorrelationId` e suporte a idempotência/outbox.
- `OrderPaidEventHandler` deixa de gravar ledger órfão sem baixa de lote.

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.2
- [`ADR-019`](./ADR-019-produtos-lotes-estoque.md), [`ADR-016`](./ADR-016-carteira-vacinacao.md)
