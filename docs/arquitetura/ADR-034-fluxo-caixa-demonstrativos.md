# ADR-034: Fluxo de caixa e demonstrativos (Fase 7.4)

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 7.2 entregou AP/AR e projeção previsto vs realizado (ADR-032). A 7.3 separou caixa operacional (gaveta/sangria) no Sales e conciliação TEF no Finance (ADR-033). O roadmap 7.4 exige fluxo de caixa diário/mensal, DRE simplificada e export CSV/PDF com aceite: relatório mensal consistente com lançamentos AP/AR.

## Decisão

1. **Sem tabelas novas:** relatórios são consultas sobre `FinancialTitle`, `TitleAllocation` e `FinancialCategory`.
2. **Fluxo de caixa (regime de caixa):** entradas/saídas realizadas por `PaidAt` (Settlement − Reversal); previsto por `DueDate` e `OpenAmount`. Não inclui movimentos de gaveta do Sales (evita duplicar vendas já liquidadas no AR).
3. **DRE simplificada (competência operacional):** receita/despesa por `IssueDate` no mês e categoria; estornos no mês reduzem a linha da categoria. Sem CMV/impostos — apuração tributária gerencial fica no módulo Fiscal ([ADR-037](./ADR-037-planejamento-fiscal.md)).
4. **Motor compartilhado:** `FinancialStatementCalculator` em Finance.Domain; API (CQRS) e clientes offline (SQLite) reutilizam a mesma lógica.
5. **Export:** `GET /api/v1/finance/statements/export` (mês calendário completo); CSV na Application; PDF via QuestPDF na Infrastructure. PDF online-only no cliente; CSV também offline.
6. **Projeção 7.2:** `GetBalanceProjectionQuery` passa a carregar títulos via `ListForStatementsAsync` e filtra realizado por `PaidAt`.

## Consequências

- Endpoints `/api/v1/finance/cash-flow`, `/dre`, `/statements/export`.
- Página `/finance/reports`; link a partir de `/finance`.
- Aceite: `MonthlyStatements_MatchAccountsPayableReceivable`; export CSV/PDF.

## Confirmação no código

- [`FinancialStatementCalculator`](../../src/Modules/Finance/Domain/Services/FinancialStatementCalculator.cs)
- [`ReportQueryHandlers`](../../src/Modules/Finance/Application/Reports/ReportQueryHandlers.cs)
- [`FinanceEndpointExtensions`](../../src/API/Extensions/FinanceEndpointExtensions.cs)
- [`FinanceReports.razor`](../../src/Clients/SharedUI/Pages/FinanceReports.razor)

## Relacionados

- [ADR-032](./ADR-032-contas-pagar-receber.md), [ADR-033](./ADR-033-caixa-sangrias-conciliacao.md)
- [`docs/diagramas/finance-relatorios.mmd`](../diagramas/finance-relatorios.mmd)
