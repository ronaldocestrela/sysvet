# ADR-037: Planejamento fiscal (Fase 7.7)

## Status
Accepted

## Data
2026-09-21

## Contexto

As Fases 7.5–7.6 entregaram emissão NF-e, NFS-e e NFC-e ([ADR-035](./ADR-035-provedor-fiscal-zeus-openac.md), [ADR-036](./ADR-036-nfce-contingencia-offline.md)). O roadmap 7.7 exige relatórios tributários por período, simulação gerencial Simples vs Presumido e exportação para contabilidade. A DRE financeira (7.4) exclui impostos de propósito ([ADR-034](./ADR-034-fluxo-caixa-demonstrativos.md)).

O modelo relacional guarda totais comerciais e tags de linha (CFOP, CSOSN); não há ICMS/PIS/COFINS/ISS persistidos fora do XML em blob.

## Opções consideradas

1. **Parse de XML autorizado** — breakdown tributário fiel; alto custo, fakes de CI sem XML rico.
2. **Snapshot de impostos na autorização** — exige migration e alteração de emissão.
3. **Relatório gerencial sobre totais + tabelas LC 123 simplificadas** — alinhado a 5 SP e ao aceite de export contábil.

## Decisão

1. **Sem tabelas novas:** consultas sobre `FiscalDocument`, itens e `IssuerProfile`.
2. **Competência:** receita por `AuthorizedAt`; cancelamento no período = estorno (`CancelledAt`).
3. **Receita:** NF-e + NFC-e = mercadorias; NFS-e = serviços; ISS estimado = serviços × `DefaultIssRate`.
4. **Simulação:** Simples (Anexos I/III — primeiras faixas por RBT12) vs Presumido cumulativo (sem ICMS presumido); disclaimer obrigatório.
5. **RBT12:** 12 meses calendário terminando em `to`.
6. **API:** `GET /api/v1/fiscal/planning`, `GET /api/v1/fiscal/planning/export` (mês completo; CSV Application, PDF QuestPDF Infrastructure).
7. **Online-only** (`Fiscal.Read`); sem espelho offline nesta fatia.
8. **Motor:** `FiscalPlanningCalculator` em Fiscal.Domain (padrão `FinancialStatementCalculator`).

## Consequências

- `ListForPlanningAsync` no repositório fiscal; filtro de datas em memória após carga (compatível SQLite/SQL Server).
- UI `/fiscal/planning`; link na listagem fiscal.
- Aceite: `PeriodTaxReport_MatchesAuthorizedDocuments`; `ExportFiscalPlanning_ReturnsCsv` / `_ReturnsPdf`.

## Confirmação no código

- [`FiscalPlanningCalculator`](../../src/Modules/Fiscal/Domain/Services/FiscalPlanningCalculator.cs)
- [`PlanningQueryHandlers`](../../src/Modules/Fiscal/Application/Planning/PlanningQueryHandlers.cs)
- [`FiscalEndpointExtensions`](../../src/API/Extensions/FiscalEndpointExtensions.cs)
- [`FiscalPlanning.razor`](../../src/Clients/SharedUI/Pages/FiscalPlanning.razor)

## Relacionados

- [ADR-034](./ADR-034-fluxo-caixa-demonstrativos.md), [ADR-035](./ADR-035-provedor-fiscal-zeus-openac.md), [ADR-036](./ADR-036-nfce-contingencia-offline.md)
- [`docs/diagramas/fiscal-planejamento.mmd`](../diagramas/fiscal-planejamento.mmd)
