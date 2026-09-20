# ADR-029: Pacotes, kits e pré-pagos (PDV)

## Status
Accepted

## Data
2026-09-19

## Contexto
A fase 6.5 exige kits de produtos (explosão de estoque no pay), pacotes pré-pagos com saldo por pet e abatimento idempotente ao consumir serviço. O Petshop (6.6) ainda não existe; o consumo deve ser invocável via contrato Core (como estoque) e via API para aceite.

## Opções consideradas
1. **Pacotes no Petshop** — mistura cobrança (Sales) com operação estética.
2. **Pacotes e kits só no Inventory** — confunde fracionamento (ADR-022) com kit comercial.
3. **Sales como dono** — catálogo de ofertas, crédito no pay, saldo e consume via MediatR.

## Decisão
- **ProductKit** e **ServicePackage** no Sales; **PrepaidBalance** por `(PetId, ServiceCode)`.
- **OrderItemKind.Kit | Package** com `CatalogOfferId`; pay explode kit → `ConsumeStockForSaleRequest`; pacote → `PrepaidBalance.Credit` idempotente por `OrderItemId`.
- **ConsumePrepaidServicePackageRequest** no Core; handler Sales delega a `ConsumePrepaidPackageUseCommand`.
- Comissão: kit = produto; pacote = serviço (ADR-028).

## Consequências
- Migration Sales; sync pull de ofertas/saldos; push consume; falhas permanentes de saldo.
- Petshop 6.6 chama o request Core ao concluir banho.

## Confirmação no código
- [`ProductKit`](../../src/Modules/Sales/Domain/Entities/ProductKit.cs), [`PrepaidBalance`](../../src/Modules/Sales/Domain/Entities/PrepaidBalance.cs)
- [`PayOrderCommandHandler`](../../src/Modules/Sales/Application/Orders/Commands/PayOrderCommandHandler.cs)
- [`ConsumePrepaidServicePackageRequest`](../../src/Modules/Core/Application/IntegrationEvents/ConsumePrepaidServicePackageRequest.cs)

## Relacionados
- [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-026](./ADR-026-pdv-offline-sync.md), [ADR-028](./ADR-028-comissoes-descontos-devolucoes-venda.md)
- [`docs/roadmap.md`](../roadmap.md) § 6.5
