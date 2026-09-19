# ADR-023: Inventário mobile (contagem cega e aprovação)

## Status
Accepted

## Data
2026-09-19

## Contexto
A Fase 5.5 exige inventário físico parcial via leitura de código de barras no MAUI, com contagem cega, divergência visível após envio e ajuste de saldo somente após aprovação. As fases 5.2–5.4 já entregam ledger lot-aware, ajustes (`AdjustmentDirection`) e lookup por barcode na API.

## Decisão
- **Sessão online-only:** agregado `InventoryCount` + `InventoryCountLine` persistidos no SQL Server; **sem sync/outbox** (decisão de produto alinhada à NF-e 5.3).
- **Estados:** `InProgress` → `Submitted` → `Approved` | `Cancelled`. Uma sessão `InProgress` por tenant.
- **Contagem cega:** em `InProgress`, DTOs omitem `ExpectedQuantity` e `Variance`; no `Submit`, snapshot do on-hand e cálculo de divergência.
- **Aprovação:** delta aplicado = `CountedQuantity - on-hand atual` (não o snapshot), via `StockLedgerWriter` com `MovementType.Adjustment`, `StockMovementReasons.InventoryCount`, `CorrelationId = session.Id`.
- **Inventário parcial:** apenas linhas contadas são ajustadas.
- **Permissões:** `Stock.Read` (listagem/detalhe); `Stock.Write` (mutações e aprovação).
- **Clientes:** `IInventoryCountApiService` + página SharedUI `/inventory-counts`; `IBarcodeScannerService` no MAUI (prompt nativo; web usa digitação manual).

## Consequências
- Migration `InventoryCounts` / `InventoryCountLines`.
- Menu `inventory-counts` mapeado a `Stock.Read`.
- Movimentos gerados entram no pull existente de `StockMovement`.

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.5
- [`ADR-020`](./ADR-020-movimentacoes-estoque-alertas.md)
- [`ADR-021`](./ADR-021-entrada-xml-nfe-compra.md)
