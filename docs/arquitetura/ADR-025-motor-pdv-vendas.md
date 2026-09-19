# ADR-025: Motor de vendas (PDV) online — pedido, pagamentos e estoque

## Status
Accepted

## Data
2026-09-19

## Contexto
A Fase 6.1 exige PDV online com carrinho (produto e serviço), split de pagamentos (sem TEF), vínculo tutor/pet, comprovante, baixa de estoque na conclusão e conversão de orçamento clínico aprovado (ADR-017). O módulo Sales já tinha esqueleto (`Order`, `CashRegister`, pay simples); Inventory baixava estoque via `OrderPaidEvent` e **ignorava** saldo insuficiente (ADR-020). Não existe módulo Finance (7.1); receita deve ficar pendente para integração futura.

## Opções consideradas
1. **Baixa de estoque só via `OrderPaidEvent` (handler Inventory)** — assíncrono; difícil falhar a venda atomicamente quando falta estoque.
2. **`ConsumeStockForSaleRequest` (MediatR) no pay, antes do commit Sales** — mesmo padrão de contrato cross-módulo no Core; pay falha se estoque insuficiente; evento enriquecido só após sucesso.
3. **Criar módulo Finance na 6.1** — fora do escopo; atrasaria PDV.

## Decisão
- **Domínio Sales:** enums (`OrderStatus`, `OrderItemKind`, `PaymentMethod`, `FinanceIntegrationStatus`, …); `Order.Pay(IReadOnlyList<Payment>)` exige soma = total e pedido não vazio; linhas `Product` (com `ProductId`) vs `Service` (sem estoque); tutor/pet opcionais (pet exige tutor).
- **Preço:** snapshot em `OrderItem.UnitPrice` (ADR-019 — sem preço de venda no catálogo); informado no PDV ou vindo do orçamento.
- **Estoque:** `ConsumeStockForSaleRequest` em Core; handler em Inventory com **falha** em saldo/FEFO insuficiente; removido `INotificationHandler<OrderPaidEvent>` de estoque para evitar dupla baixa.
- **Pay pipeline:** `PayOrderCommandHandler` → domínio `Pay` → `Send(ConsumeStockForSaleRequest)` → persistência → `OrderPaidEvent` enriquecido (`TutorId`, pagamentos, `FinanceIntegrationStatus.Pending`) e, se `SourceQuoteId`, `ClinicalQuoteConvertedEvent` → Veterinary `MarkConverted`.
- **Caixa:** saldo atual calculado na query (abertura + pagamentos `Cash` de pedidos pagos da sessão); sangria em 7.3.
- **Clients:** checkout **online-only** (`SalesApiService`); catálogo local via `IInventoryStore`; sem outbox de vendas (6.2).
- **Finance:** `FinanceIntegrationStatus.Pending` no pedido pago; AR materializado na 7.2 ao consumir `OrderPaidEvent`.

## Consequências
- Migration Sales: `Payments`, `TutorId`/`PetId`/`SourceQuoteId`, `OrderItem.Kind`, `ProductId` nullable.
- TEF/NSU, fila offline PDV, comissões e devoluções permanecem nas fases 6.2–6.4.
- Repositório Sales anexa pagamentos explicitamente na atualização (coleção com backing field EF).

## Confirmação no código
- [`ConsumeStockForSaleRequest`](../../src/Modules/Core/Application/IntegrationEvents/ConsumeStockForSaleRequest.cs), [`ConsumeStockForSaleRequestHandler`](../../src/Modules/Inventory/Application/StockSales/ConsumeStockForSaleRequestHandler.cs)
- [`PayOrderCommandHandler`](../../src/Modules/Sales/Application/Orders/Commands/PayOrderCommandHandler.cs), [`Order`](../../src/Modules/Sales/Domain/Entities/Order.cs)
- [`SalesEndpointExtensions`](../../src/API/Extensions/SalesEndpointExtensions.cs); aceite [`SalesEndpointsTests`](../../tests/API.IntegrationTests/Sales/SalesEndpointsTests.cs)
- Clients: [`SalesApiService`](../../src/Clients/Clients.Infrastructure/Http/SalesApiService.cs), [`PosTerminal.razor`](../../src/Clients/SharedUI/Pages/Sales/PosTerminal.razor)

## Relacionados
- [ADR-001](./ADR-001-monolito-modular.md), [ADR-017](./ADR-017-orcamentos-clinicos.md), [ADR-019](./ADR-019-produtos-lotes-estoque.md), [ADR-020](./ADR-020-movimentacoes-estoque-alertas.md)
- [`docs/diagramas/pdv-motor-vendas.mmd`](../diagramas/pdv-motor-vendas.mmd)
- [`docs/roadmap.md`](../roadmap.md) § 6.1
