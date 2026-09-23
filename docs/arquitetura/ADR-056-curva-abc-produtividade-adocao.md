# ADR-056: Curva ABC, produtividade e adoção (Fase 10.3)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 10.3 exige ranking ABC de clientes e produtos, produtividade por profissional (tenant) e heatmap de adoção de módulos (Super Admin). Dashboards do dia (10.1) e métricas SaaS globais (10.2) ficam fora desta fatia.

## Opções consideradas

1. **Consultas diretas do Intelligence sobre DbContexts de Sales/Veterinary/Petshop** — viola isolamento modular; rejeitado.
2. **Intelligence + `IRequest` no Core com handlers nos módulos donos + calculadores puros** — alinhado a ADR-054; aceito.
3. **Adoção por telemetria de uso de tela** — não existe instrumentação; rejeitado — aceite usa entitlement efetivo (plano ∪ add-ons ± flags).

## Decisão

1. **`AbcCurveClassifier`** e **`ProductivityMerger`** em Intelligence.Domain; período civil `America/Sao_Paulo`, máximo 366 dias.
2. ABC por valor líquido de linha (quantidade restante × preço unitário); produtos apenas `OrderItemKind.Product`.
3. Produtividade: vendas por `PerformerUserId`; consultas `Completed` por `VeterinarianId`; banho `Completed` por `GroomerId`; receita de consulta/banho não somada fora do PDV.
4. **`ModuleAdoptionCalculator`** em Platform.Domain; matriz por tenant ativo/suspenso (não cancelado) com módulos efetivos de **`EntitlementCalculator`**.
5. API tenant: `/api/v1/intelligence/reports/*` + export CSV; plataforma: `/api/v1/platform/adoption` + export CSV.

## Consequências

- Aceite: relatórios exportáveis; adoção bate com flags/planos reais.
- Cache Redis permanece Fase 10.4.

## Relacionados

- [ADR-054](./ADR-054-intelligence-dashboards-tenant.md), [ADR-055](./ADR-055-metricas-saas-platform.md), [ADR-048](./ADR-048-planos-addons-feature-flags.md)
- [`docs/roadmap.md`](../roadmap.md) §10.3
