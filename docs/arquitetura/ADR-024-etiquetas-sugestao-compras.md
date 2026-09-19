# ADR-024: Etiquetas de produto e sugestão de compras

## Status
Accepted

## Data
2026-09-19

## Contexto
A Fase 5.6 exige geração/impressão de etiquetas (PDF/ZPL) e regra de reposição com pedido sugerido agrupado por fornecedor, exportável. O Inventory já possui `Product` com SKU/barcode, `ReorderLevel`, alertas low-stock (ADR-020) e fornecedor opcional (ADR-019). Não existe agregado de pedido de compra; Finance 7.2 ainda não implementa AP.

## Opções consideradas
1. **CSS print no cliente (ADR-016)** — sem PDF server-side; não atende ZPL para impressoras térmicas nem aceite explícito de PDF na API.
2. **API PDF + ZPL (QuestPDF + encoder ZPL puro)** — download via `Results.File`; labels online-only como NF-e/inventário.
3. **Persistir rascunho de pedido** — histórico útil; fora do escopo 8 SP desta fase.

## Decisão
- **`Product.TargetStock`:** estoque-alvo para sugestão; `0` usa apenas `ReorderLevel` como alvo efetivo. Validação: `TargetStock >= ReorderLevel` quando ambos &gt; 0.
- **Sugestão:** projeção CQRS (`ListPurchaseSuggestionsQuery`, `ExportPurchaseSuggestionsQuery` CSV); `PurchaseSuggestionCalculator` reutiliza `StockAlertClassifier.IsLowStock`; quantidade = `ceil((alvo - saldo) / embalagem) * embalagem`.
- **Etiquetas:** `GenerateProductLabelsQuery`; ZPL via `ZplLabelEncoder` (Domain); PDF via `IProductLabelPdfRenderer` (QuestPDF + ZXing na Infrastructure). Layout 60×40 mm, Code128, nome + SKU.
- **Permissões:** `Products.Read` (labels); `Stock.Read` (sugestão/export). Sem permissão nova.
- **Sync:** apenas `TargetStock` no pull/push de produtos; geração de etiqueta e relatório **online-only** (sem outbox).

## Consequências
- Nova dependência QuestPDF (licença Community documentada) e ZXing.Net na Infrastructure.
- Migration `Products.TargetStock`; DTOs sync e SQLite offline atualizados.
- Pedido formal ao fornecedor permanece para fase futura (Finance/marketplace).

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.6
- [`ADR-016`](./ADR-016-carteira-vacinacao.md), [`ADR-020`](./ADR-020-movimentacoes-estoque-alertas.md)
