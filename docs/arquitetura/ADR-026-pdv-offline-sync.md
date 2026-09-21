# ADR-026: PDV 100% offline — fila local e sync de vendas

## Status
Accepted

## Data
2026-09-19

## Contexto
A Fase 6.1 entregou o motor PDV **online-only** (ADR-025). Clínicas precisam concluir vendas sem rede, com sync idempotente e tratamento de conflito de estoque quando o servidor recusa a baixa (ADR-002).

## Opções consideradas
1. **Sequência reservada na API** — numeração legível; depende de conectividade para reservar faixa.
2. **UUID no cliente + outbox** — alinhado a tutor/pet/movimentos; idempotência via `OutboxMessage.Id`.
3. **Outbox com `RegisterStockMovement` local + pay no servidor** — risco de dupla baixa ou movimentos órfãos.

## Decisão
- **Identidade:** `OrderId` e `CashRegisterId` gerados no cliente (`Guid`); `OutboxMessage.Id` = `IdempotencyKey` da operação.
- **Store único:** `ISalesStore` persiste SQLite + outbox sempre; UI não usa `SalesApiService` para checkout.
- **Pay local:** pedido `Paid` + débito otimista de lotes/saldo local (FEFO); **sem** `RegisterStockMovementCommand` na outbox — o servidor cria movimento `Sale` em `ConsumeStockForSaleRequest` no pay.
- **Conflito:** `Order.InsufficientStock` / `ProductBalance.InsufficientFunds` = falha **permanente** (dead-letter); venda permanece paga localmente; operador resolve manualmente (devoluções 6.4).
- **Plugin sync:** `SalesSyncPushHandler` + `SalesSyncChangeFeedContributor`; pull LWW por `UpdatedAt`.
- **NFC-e / sequência fiscal:** implementado na 7.6 ([ADR-036](./ADR-036-nfce-contingencia-offline.md)): outbox `TransmitNfceCommand` após pay, série por dispositivo, pull de status.

## Consequências
- Domain: `Order.Create(id, …)`, `CashRegister.Open(id, …)`.
- Application: `CreateOrderCommand.OrderId`, `OpenCashRegisterCommand.CashRegisterId`; replay idempotente (pedido já pago / caixa já fechado).
- Clients: migration SQLite; correção do early-exit de pull quando só há deltas de inventory/sales.
- `RequestSync()` no worker após mutações de vendas quando online.

## Relacionados
- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-025](./ADR-025-motor-pdv-vendas.md)
- [`docs/diagramas/pdv-offline-sync.mmd`](../diagramas/pdv-offline-sync.mmd), [`docs/diagramas/fiscal-nfce-contingencia.mmd`](../diagramas/fiscal-nfce-contingencia.mmd)
- [`docs/roadmap.md`](../roadmap.md) § 6.2
