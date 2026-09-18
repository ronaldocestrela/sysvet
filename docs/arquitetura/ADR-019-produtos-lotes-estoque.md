# ADR-019: Cadastro de produtos, lotes e saldo por lote

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 5.1 exige catálogo de produtos com SKU, código de barras, fornecedor, categorias, campos fiscais básicos e **múltiplos lotes** com saldo e custo por lote. O módulo Inventory já possuía `Product`, `ProductBalance` agregado por produto e movimentações com `BatchNumber` textual, sem agregado de lote.

## Opções consideradas
1. **Saldo apenas em movimentações (event sourcing)** — kardex completo na 5.2; cadastro 5.1 ficaria pesado.
2. **`ProductLot` com quantidade on-hand + projeção `ProductBalance`** — leitura rápida; movimentações referenciam lote na 5.2.
3. **Fornecedor no Finance** — adiado; compras 5.3 precisam de cadastro mínimo no Inventory.

## Decisão
- Entidades `Supplier`, `Product` (expandido), `ProductLot`; VOs `Sku`, `Barcode`, `Ncm`, `Money` no Inventory (sem referência a Sales).
- Saldo por lote em `ProductLot.Quantity`; `ProductBalance.TotalQuantity` = soma dos lotes ativos.
- **Custo médio ponderado** no produto: Σ(qty × unitCost) / Σ(qty) sobre lotes ativos com qty > 0.
- Saldo inicial de lote gera `StockMovement` tipo `In` com motivo `OpeningBalance`.
- Fiscais básicos no produto: NCM (8 dígitos), CEST opcional, origem da mercadoria (0–8). CFOP/CST/alíquotas permanecem no Fiscal (Fase 7).
- Sem preço de venda no catálogo (ADR-017).
- Sync: plugin Inventory (`ISyncChangeFeedContributor` / `ISyncPushHandler`) para Products, ProductLots, Suppliers.
- `RegisterStockMovement` na 5.1 continua no saldo de produto; **5.2** unificou movimentações lot-aware (ver [`ADR-020`](./ADR-020-movimentacoes-estoque-alertas.md)).

## Consequências
- Migration `AddProductLotsAndCatalog` altera schema de `Products` e adiciona tabelas.
- Permissões `Products.Read/Write`; recepção com escrita de catálogo; veterinário leitura.

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.1
- [`ADR-002`](./ADR-002-estrategia-de-sync.md), [`ADR-017`](./ADR-017-orcamentos-clinicos.md)
