# Módulo Commerce

E-commerce e marketplaces (Fase 8.8): ofertas com preço de venda, loja pública, pedidos online com baixa de estoque e PoC Mercado Livre.

## Escopo

- `ProductOffer`, `OnlineOrder`, outbox `MarketplaceSyncJob`
- API staff `/api/v1/commerce/*`
- API pública `/api/v1/public/clinic-sites/{slug}/store/*`
- Integração estoque via `ConsumeStockForSaleRequest` (Core)
- Persistência via `TransactionBehavior`: `CommerceDbContext` registrado como `IUnitOfWork`
- Client `ClinicSiteWeb` rotas `/loja`; staff SharedUI `/commerce/offers` e `/commerce/orders`

## Fora de escopo

Gateway de pagamento, NFC-e da loja, Shopee, checkout autenticado TutorPortal.

## Referências

- [ADR-045](../../../docs/arquitetura/ADR-045-commerce-ecommerce-marketplace.md)
- [roadmap § 8.8](../../../docs/roadmap.md)
