# ADR-032: Contas a pagar e receber (Fase 7.2)

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 7.1 entregou o esqueleto do módulo Finance. Vendas (ADR-025) já publicam `OrderPaidEvent` com `FinanceIntegrationStatus.Pending`; compras XML (ADR-021) publicam `PurchaseInvoiceImportedEvent` com `ApIntegrationStatus.Pending`. Estornos e devoluções expõem `OrderPaymentRefundedEvent` e `OrderReturnedEvent` (ADR-027/028).

## Decisão

1. **Domínio Finance:** agregado `FinancialTitle` (AP/AR), `FinancialCategory`, `CostCenter`, alocações `TitleAllocation` (baixa/estorno).
2. **Integração via Core (ADR-001):** handlers `INotificationHandler` no Finance; requests `MarkOrderFinanceLinkedRequest` (Sales) e `MarkPurchaseInvoiceApLinkedRequest` (Inventory) após materializar títulos.
3. **AR de PDV:** um recebível por pedido pago, **liquidado** na criação (split de pagamentos vira alocações). Cartão não fica em aberto; alocações de débito/crédito persistem **NSU** em `ExternalReference` (conciliação PoC entregue na 7.3, ADR-033).
4. **AP de NF-e:** um payable por duplicata (`cobr/dup`); sem duplicatas, título único no total da nota.
5. **Permissões:** `Finance.Read` / `Finance.Write`; menu `finance`; Receptionist + Admin.
6. **Projeção 7.2:** previsto (saldo em aberto por vencimento) vs realizado (alocações no período); ledger por contraparte — não é DRE (7.4).

## Consequências

- Migration `AddAccountsPayableReceivable`; endpoints `/api/v1/financial-*`.
- Sync pull/push de títulos e catálogos; SQLite offline espelha títulos (baixa manual via outbox).
- Aceite: `PayOrder_ShouldDebitStockAndMarkFinanceLinked`; `ConfirmPurchaseXml_CreatesPayablesFromDuplicates`.

## Confirmação no código

- [`OrderPaidIntegrationHandler`](../../src/Modules/Finance/Application/Integration/OrderPaidIntegrationHandler.cs)
- [`PurchaseInvoiceImportedIntegrationHandler`](../../src/Modules/Finance/Application/Integration/PurchaseInvoiceImportedIntegrationHandler.cs)
- [`FinanceEndpointExtensions`](../../src/API/Extensions/FinanceEndpointExtensions.cs)

## Relacionados

- [ADR-021](./ADR-021-entrada-xml-nfe-compra.md), [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-027](./ADR-027-pagamentos-tef.md), [ADR-033](./ADR-033-caixa-sangrias-conciliacao.md)
- [`docs/diagramas/finance-ap-ar.mmd`](../diagramas/finance-ap-ar.mmd)
