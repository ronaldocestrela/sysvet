# ADR-045: E-commerce e marketplaces — Fase 8.8

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.8 exige loja virtual sincronizada com estoque físico e preços do backoffice, pedidos online com baixa automática de estoque e PoC de marketplace (Mercado Livre). Inventory (ADR-019) não possui preço de venda; Sales `Order` (ADR-025) exige caixa aberto e cobre PDV. ClinicSite (ADR-044) é vitrine marketing; TutorPortal (ADR-041) é canal autenticado do tutor.

## Opções consideradas

1. **Estender Sales.Order com canal web** — exige relaxar `CashRegisterId` e acopla PDV a e-commerce; rejeitado.
2. **Preço no Inventory** — quebra ADR-019; rejeitado.
3. **Módulo `Commerce` + `ProductOffer` + `OnlineOrder` + loja em ClinicSiteWeb** — aceito; integração estoque via `ConsumeStockForSaleRequest` (Core); sem `OrderPaidEvent` (Finance permanece ligado ao PDV).

## Decisão

1. Módulo **`Commerce`** (`Domain` / `Application` / `Infrastructure`) com `ProductOffer` (preço de venda por produto), `OnlineOrder` (retirada na clínica ou ML), outbox `MarketplaceSyncJob`.
2. Confirmação de pedido da loja chama **`ConsumeStockForSaleRequest`** com `CorrelationId = OnlineOrder.Id`; cancelamento pós-confirmação usa **`RestoreStockForSaleReturnRequest`**.
3. Contrato **`GetSellableProductSnapshotsRequest`** no Core; handler em Inventory (estoque disponível + metadados de catálogo).
4. API staff `/api/v1/commerce/*`; API pública `/api/v1/public/clinic-sites/{slug}/store/*` com `ClinicSitePublicTenantFilter`.
5. PoC ML: `IMarketplaceChannel`, `FakeMarketplaceChannel` (testes), `MercadoLivreChannel` (HTTP fino); índice global `MarketplaceSellerIndex` (`MercadoLivreUserId → TenantId`) para webhook.
6. **Sem gateway de pagamento** nesta fatia: estoque baixa na confirmação; pagamento na retirada/PDV manual fora do escopo.

## Consequências

- Aceite: `EcommerceOrder_DebitsStock_WhenConfirmed`; `OfferPrice_ReflectsBackofficeChange_OnPublicCatalog`; `MercadoLivreInboundOrder_DebitsStock`.
- Não registrar o mesmo pedido no PDV (duplicaria baixa de estoque).
- Shopee, frete, reserva de carrinho e checkout TutorPortal autenticado ficam para evolução.

## Confirmação no código

- [`CommerceEndpointExtensions`](../../src/API/Extensions/CommerceEndpointExtensions.cs)
- [`ConsumeStockForSaleRequest`](../../src/Modules/Core/Application/IntegrationEvents/ConsumeStockForSaleRequest.cs)
- [`GetSellableProductSnapshotsRequest`](../../src/Modules/Core/Application/IntegrationEvents/GetSellableProductSnapshotsRequest.cs)
- [`ClinicSiteWeb/Pages/StorePage.razor`](../../src/Clients/ClinicSiteWeb/Pages/StorePage.razor)

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-019](./ADR-019-produtos-lotes-estoque.md), [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-044](./ADR-044-clinic-site.md)
- [`docs/diagramas/commerce-ecommerce.mmd`](../diagramas/commerce-ecommerce.mmd)
