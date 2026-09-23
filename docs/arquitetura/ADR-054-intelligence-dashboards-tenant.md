# ADR-054: Intelligence — dashboards tenant (Fase 10.1)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 10.1 exige módulo `Intelligence` com painel do dia (vendas e serviços), widgets filtrados por perfil de acesso e entitlement comercial, aceite de carregamento &lt; 3s em tenant médio. Métricas SaaS globais (10.2) e curva ABC (10.3) ficam fora desta fatia.

## Opções consideradas

1. **Consultas diretas no Intelligence sobre DbContexts de Sales/Petshop** — viola isolamento modular; rejeitado.
2. **Módulo Intelligence + `IRequest` de agregação no Core (handlers nos módulos donos)** — alinhado a ADR-001/004 e `GetPaidOrderFiscalSnapshotRequest`; aceito.
3. **SignalR para tempo real** — escopo 10.1 usa poll 30s e recálculo na leitura; rejeitado nesta entrega.

## Decisão

1. Módulo **`Intelligence`** (`Domain` / `Application` / `Infrastructure`) com `ProfileDashboardLayout` por `AccessProfile`, API `/api/v1/intelligence/*`, `CommercialModule.Intelligence`.
2. KPIs do dia via requests `GetSalesTodayKpisRequest`, `GetSalesByHourKpisRequest`, `GetGroomingTodayKpisRequest`, `GetClinicalAppointmentsTodayKpisRequest`, `GetOnlineOrdersTodayKpisRequest` em `Core.Application.IntegrationEvents`; handlers em Infrastructure de Sales, Petshop, Veterinary e Commerce.
3. Dia operacional fixo `America/Sao_Paulo` (`BusinessDayRange`).
4. Layout default por `BaseRole`; override persistido por perfil; widgets ocultos quando módulo-fonte não está no entitlement.
5. Permissões `Intelligence.Read` e `Intelligence.LayoutWrite`; home Blazor consome `GET /dashboard` com poll 30s.

## Consequências

- Aceite: `Dashboard_LoadsTodayKpis_UnderThreeSeconds`; layouts inválidos falham no domínio.
- Planos Pro/Hospital incluem Intelligence; add-on para Starter.
- Cache Redis permanece na Fase 10.4.

## Confirmação no código

- [`IntelligenceEndpointExtensions`](../../src/API/Extensions/IntelligenceEndpointExtensions.cs)
- [`GetTenantDashboardQueryHandler`](../../src/Modules/Intelligence/Application/Dashboard/Queries/GetTenantDashboardQueryHandler.cs)
- [`Dashboard.razor`](../../src/Clients/SharedUI/Pages/Dashboard.razor)

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-009](./ADR-009-access-profiles.md), [ADR-048](./ADR-048-planos-addons-feature-flags.md)
- [`docs/roadmap.md`](../roadmap.md) §10.1
