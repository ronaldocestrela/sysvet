# ADR-022: Perdas, fracionamento e devoluções ao fornecedor

## Status
Accepted

## Data
2026-09-19

## Contexto
A Fase 5.4 exige motivos auditáveis de perda, abertura de embalagens com saldo fracionado rastreável e devolução de compras ao fornecedor. A 5.2 já entrega ledger lot-aware, transferência e sync de movimentações.

## Decisão
- **Perdas:** `MovementType.Out` com códigos `LossExpired`, `LossDamage`, `InternalConsumption`, `Donation`; entrada de UI via whitelist `StockLossReasons`.
- **Fracionamento:** `Product.UnitsPerPackage` (> 1); abrir N embalagens transfere `N × UnitsPerPackage` do lote lacrado para lote `{LotNumber}-F` com `IsFractional = true` (par Out/In, motivo `Fractionation`, `CorrelationId` compartilhado). Reutiliza mecânica de transferência; cria lote `-F` se ausente.
- **FEFO:** `LotAllocationService` consome lotes `IsFractional` antes de lacrados.
- **Devolução:** comando `RegisterSupplierReturn` → `Out` + `SupplierReturn`, `SupplierId` na movimentação, evento `SupplierReturnRegisteredEvent` para Finance futuro (sem AP nesta fase).
- **Permissões:** `Stock.Write` para perda/fracionamento/devolução; `Products.Write` para `UnitsPerPackage`.
- **Sync:** novos campos em pull de produtos/lotes/movimentos; push dos três commands idempotentes.

## Consequências
- Migration Inventory + SQLite offline para `UnitsPerPackage`, `IsFractional`, `Notes`, `SupplierId` em movimentos.
- UI SharedUI estende movimentações e detalhe de produto.

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.4
- [`ADR-020`](./ADR-020-movimentacoes-estoque-alertas.md)
