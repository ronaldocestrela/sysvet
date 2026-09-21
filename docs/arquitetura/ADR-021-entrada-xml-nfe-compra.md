# ADR-021: Entrada de NF-e de compra via XML

## Status
Accepted

## Data
2026-09-18

## Contexto
A Fase 5.3 exige importação de XML de NF-e de entrada com mapeamento assistido de fornecedor/produto e geração de movimentações de estoque conferíveis. O Inventory já possui fornecedores (CNPJ), catálogo, lotes e `RegisterStockMovement` com motivo `Purchase` (ADR-019/020). O módulo Fiscal trata emissão (Fase 7.5); Finance ainda não existe (7.2 gerará AP a partir de compras).

## Opções consideradas
1. **Parser no Fiscal** — reutilizaria layout NF-e, mas mistura entrada de compra (estoque) com emissão tributária.
2. **Importação one-shot sem rascunho** — menos UX para linhas não mapeadas.
3. **Parse + Confirm no Inventory, blob XML, evento AP** — alinhado a ADR-017 (conversão assistida) e ADR-015 (armazenamento de binários).

## Decisão
- **Bounded context:** parser e orquestração em **Inventory** (`Application/PurchaseImports`). Fiscal permanece emissão.
- **Fluxo em dois passos:** `ParsePurchaseNfeXml` persiste agregado `PurchaseInvoiceImport` em `Draft`, grava XML via `IBlobStorage` (`{schema}/purchases/{yyyy}/{MM}/{importId}.xml`). `ConfirmPurchaseNfeImport` aplica mapeamentos explícitos do usuário, cria lotes/movimentações `In`/`Purchase` com `CorrelationId` = import id, persiste `SupplierProductMapping` e confirma o agregado.
- **Idempotência:** `AccessKey` (chNFe, 44 dígitos) única por tenant; reimportação de nota já confirmada retorna conflito.
- **Contas a pagar (Fase 7.2):** após confirm, publicar `PurchaseInvoiceImportedEvent` em Core.Application; consumer Finance cria AP e envia `MarkPurchaseInvoiceApLinkedRequest` → `ApIntegrationStatus.Linked` (ADR-032).
- **Permissões:** `PurchaseImports.Read` / `PurchaseImports.Write`. Recepção recebe Write sem `Stock.Write`.
- **Sync offline:** importação é **online-first** (upload na API). Movimentações/produtos/lotes/fornecedores resultantes seguem sync existente; agregado de importação e XML **não** entram no pull nesta fase.

## Consequências
- Migration `AddPurchaseInvoiceImports` e repositórios dedicados.
- `RegisterStockMovementCommand` passa a aceitar `CorrelationId` opcional.
- Menu `purchase-imports`; correção de `suppliers` no `MenuCatalog`.

## Referências
- [`docs/roadmap.md`](../roadmap.md) § 5.3, § 7.2
- [`ADR-019`](./ADR-019-produtos-lotes-estoque.md), [`ADR-020`](./ADR-020-movimentacoes-estoque-alertas.md), [`ADR-015`](./ADR-015-blob-storage-clinico.md), [`ADR-017`](./ADR-017-orcamentos-clinicos.md)
