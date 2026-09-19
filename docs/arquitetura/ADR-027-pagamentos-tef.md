# ADR-027: Pagamentos eletrônicos, TEF (PoC) e estorno no PDV

## Status
Accepted

## Data
2026-09-20

## Contexto
As fases 6.1–6.2 entregaram PDV com split de pagamentos manual (sem NSU) e sync offline (ADR-025, ADR-026). A fase 6.3 exige integração com maquininha (PoC), registro de débito/crédito/Pix com NSU e estorno parcial/total refletido no caixa, sem devolução de estoque (6.4) nem módulo Finance (7.x).

## Opções consideradas
1. **Adapter de acquirer real na 6.3** — depende de vendor; atrasa PoC e testes offline.
2. **Porta `IPaymentTerminal` + simulador offline** — mesmo contrato para Stone/PayGo/etc. depois; NSU sintético; replay de sync não reautoriza.
3. **TEF só na API (online)** — quebra PDV 100% offline da 6.2.

## Decisão
- **Porta:** `IPaymentTerminal` (`AuthorizeAsync` / `RefundAsync`) em `Sales.Domain.Payments`; implementação **`SimulatedPaymentTerminal`** (NSU 12 dígitos, `Provider = Simulator`), registrada na API e nos clients.
- **Domínio:** `Payment` com metadados TEF (`Nsu`, `AuthorizationCode`, `Provider`, …); `PaymentRefund`; `Order.RefundPayment` (estorno ≠ devolução de itens); `OrderStatus.PartiallyRefunded` / `Refunded`.
- **Pay:** métodos eletrônicos sem NSU → terminal antes de `Order.Pay`; payload offline com NSU → **skip** terminal (replay sync). Falha de estoque após authorize → `RefundAsync` compensatório.
- **Estorno:** `RefundOrderPaymentCommand` + `POST .../payments/{id}/refund`; persistência via `PersistRefundAsync` (evita conflito de `RowVersion` no aggregate).
- **Caixa:** `CurrentBalance = abertura + líquido em dinheiro`; `MethodTotals` (bruto / estornado / líquido) por forma na sessão.
- **Sync:** outbox `RefundOrderPaymentCommand`; pull/push estende DTOs de pagamento e refunds.
- **Clients:** `OfflineSalesStore` autoriza via terminal local; `ISalesStore.RefundOrderPaymentAsync`; UI comprovante (NSU + estornar) e caixa com breakdown.

## Consequências
- Migrations Sales (`PaymentsTef`) e SQLite cliente (`OfflinePaymentsTef`).
- `OrderPaymentRefundedEvent` publicado; sem consumer Finance até 7.2.
- Adapters reais plugam em `IPaymentTerminal` sem alterar domínio/application.

## Confirmação no código
- [`IPaymentTerminal`](../../src/Modules/Sales/Domain/Payments/IPaymentTerminal.cs), [`SimulatedPaymentTerminal`](../../src/Modules/Sales/Domain/Payments/SimulatedPaymentTerminal.cs)
- [`PayOrderCommandHandler`](../../src/Modules/Sales/Application/Orders/Commands/PayOrderCommandHandler.cs), [`RefundOrderPaymentCommandHandler`](../../src/Modules/Sales/Application/Orders/Commands/RefundOrderPaymentCommandHandler.cs)
- [`SalesEndpointExtensions`](../../src/API/Extensions/SalesEndpointExtensions.cs); [`OfflineSalesStore`](../../src/Clients/Clients.Infrastructure/Sales/OfflineSalesStore.cs)
- Aceite: [`SalesEndpointsTests`](../../tests/API.IntegrationTests/Sales/SalesEndpointsTests.cs) (NSU Pix + estorno cash no caixa)

## Relacionados
- [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-026](./ADR-026-pdv-offline-sync.md)
- [`docs/diagramas/pdv-pagamentos-tef.mmd`](../diagramas/pdv-pagamentos-tef.mmd)
- [`docs/roadmap.md`](../roadmap.md) § 6.3
