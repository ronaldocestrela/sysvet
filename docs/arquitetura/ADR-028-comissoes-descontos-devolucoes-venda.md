# ADR-028: Comissões, descontos e devoluções de venda no PDV

## Status
Accepted

## Data
2026-09-19

## Contexto
As fases 6.1–6.3 entregaram motor PDV, sync offline e TEF com estorno de pagamento sem devolução de estoque (ADR-025–027). A fase 6.4 exige desconto com limite por perfil de acesso, motor de comissões por vendedor/veterinário/tosador e devolução de itens com recomposição de estoque e estorno proporcional no caixa, mantendo PDV offline-first.

## Opções consideradas
1. **Desconto por linha** — flexível, mas complica split de pagamento e comissão proporcional.
2. **Desconto percentual no pedido + teto em `AccessProfile`** — alinhado a perfis customizáveis (ADR-009); validação no create/pay.
3. **Devolução só via estorno manual** — não repõe estoque; inaceitável para aceite 6.4.
4. **Devolução dedicada + `RestoreStockForSaleReturnRequest`** — simétrico a `ConsumeStockForSaleRequest`; estorno de caixa reutiliza `RefundPayment` + TEF.

## Decisão
- **Desconto:** `Order.DiscountPercent` (0–100); `TotalAmount = Subtotal - DiscountAmount` (2 casas); `Pay` exige soma = líquido. Teto `AccessProfile.MaxDiscountPercent` (Admin seed 100, demais 0); exposto em `/api/v1/auth/me`. Erro `Order.DiscountExceedsProfileLimit` = falha permanente de sync.
- **Comissão:** `CommissionRule` por tenant (`Role` × `AppliesTo` × `RatePercent`); `CommissionCalculator` no pay gera `CommissionAccrual` snapshot. Vendedor = `SellerUserId` (default operador do caixa); performer opcional por linha de serviço. CRUD regras: policy `Admin`.
- **Devolução:** `ReturnOrderCommand` → `SaleReturn` + linhas; status `PartiallyReturned` / `Returned`; valor a estornar = parcela do líquido; estoque via restore idempotente (`CorrelationId = ReturnId`); comissões `Reversed` na mesma proporção.
- **Estoque:** `ConsumeStockForSale` grava `CorrelationId = OrderId`; restore credita mesmos lotes das saídas `Sale` do pedido.
- **Sync:** outbox `ReturnOrderCommand`; pull estende pedido, returns, accruals e regras de comissão.

## Consequências
- Migrations Sales (`CommissionsDiscountsReturns`) e Core (`AccessProfileMaxDiscount`); SQLite cliente.
- `OrderReturnedEvent` publicado; Finance continua pendente até 7.2.
- Estorno 6.3 permanece sem movimento de estoque.

## Confirmação no código
- [`Order`](../../src/Modules/Sales/Domain/Entities/Order.cs), [`CommissionCalculator`](../../src/Modules/Sales/Domain/Services/CommissionCalculator.cs)
- [`ReturnOrderCommandHandler`](../../src/Modules/Sales/Application/Orders/Commands/ReturnOrderCommandHandler.cs), [`RestoreStockForSaleReturnRequestHandler`](../../src/Modules/Inventory/Application/StockSales/RestoreStockForSaleReturnRequestHandler.cs)
- [`SalesEndpointExtensions`](../../src/API/Extensions/SalesEndpointExtensions.cs); [`OfflineSalesStore`](../../src/Clients/Clients.Infrastructure/Sales/OfflineSalesStore.cs)
- Aceite: [`SalesEndpointsTests`](../../tests/API.IntegrationTests/Sales/SalesEndpointsTests.cs)

## Relacionados
- [ADR-009](./ADR-009-access-profiles.md), [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-026](./ADR-026-pdv-offline-sync.md), [ADR-027](./ADR-027-pagamentos-tef.md)
- [`docs/diagramas/pdv-comissoes-devolucoes.mmd`](../diagramas/pdv-comissoes-devolucoes.mmd)
- [`docs/roadmap.md`](../roadmap.md) § 6.4
