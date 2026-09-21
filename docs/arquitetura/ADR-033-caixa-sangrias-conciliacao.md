# ADR-033: Caixa, sangrias e conciliação TEF (Fase 7.3)

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 6.x entregou `CashRegister` no Sales (abertura/fechamento, saldo = abertura + Cash líquido, ADR-025/027). A 7.2 liquidou AR no pay sem NSU persistido na alocação (conciliação adquirente = 7.3, ADR-032).

## Decisão

1. **Caixa operacional no Sales:** sangrias (`Drop`) e suprimentos (`Supply`) como filhos de `CashRegister`; esperado na gaveta = `Opening + CashNet − Drops + Supplies`.
2. **Fechamento:** persiste `ExpectedClosingBalance` e contado; `Variance = contado − esperado`; não bloqueia quebra.
3. **Finance:** `TitleAllocation.ExternalReference` (NSU); lote `CardReconciliationBatch` com match PoC por NSU + valor (Debit/Credit); import JSON online-only.
4. **Integração:** Finance não referencia Sales; match consulta alocações AR já materializadas.

## Consequências

- Migrations Sales (`CashMovements`, esperado no close) e Finance (NSU, conciliação).
- Sync pull/push de movimentos; conciliação só via API.

## Confirmação no código

- [`CashRegister`](../../src/Modules/Sales/Domain/Entities/CashRegister.cs), [`RecordCashMovementCommand`](../../src/Modules/Sales/Application/CashRegisters/Commands/RecordCashMovementCommand.cs)
- [`ImportCardStatementCommandHandler`](../../src/Modules/Finance/Application/Reconciliation/ImportCardStatementCommandHandler.cs)

## Relacionados

- [ADR-025](./ADR-025-motor-pdv-vendas.md), [ADR-027](./ADR-027-pagamentos-tef.md), [ADR-032](./ADR-032-contas-pagar-receber.md)
- [`docs/diagramas/finance-caixa-conciliacao.mmd`](../diagramas/finance-caixa-conciliacao.mmd)
