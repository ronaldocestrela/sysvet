# ADR-040: Campanhas segmentadas e NPS — Fase 8.3

## Status
Accepted

## Data
2026-09-22

## Contexto

A Fase 8.3 exige campanhas para inativos (90 dias), NPS pós-atendimento com score/comentários reportáveis e frequência de retorno. As fases 8.1–8.2 entregaram outbox SQL, lembretes transacionais e opt-out por canal ([ADR-038](./ADR-038-automations-outbox.md), [ADR-039](./ADR-039-lembretes-smtp-evolution.md)).

## Opções consideradas

1. **Novo módulo Marketing** — duplicaria fila e templates; rejeitado.
2. **Eventos de domínio no `Appointment.Complete`** — acopla Veterinary a Automations; rejeitado em favor de scan periódico (mesmo padrão dos lembretes).
3. **Opt-out de marketing no Tutor (Core)** — alteraria sync offline; rejeitado — flag `MarketingEnabled` em `TutorMessagingPreference` (schema Automations).

## Decisão

1. **Agregados** `Campaign`, `CampaignRun`, `NpsInvite` no `AutomationsDbContext`.
2. **Segmentos fixos:** `Inactive90Days` (lançamento manual `LaunchCampaign`) e `PostAppointment` (`CampaignScheduler` + `CampaignScanService`).
3. **Disparo** reutiliza `MessageJob`, templates `campaign.inactive` e `nps.request`, `CampaignDispatcher` (espelho do `ReminderPlanner`).
4. **Marketing consent:** jobs cujo template começa com `campaign.` ou `nps.` exigem `MarketingEnabled` no processor e no dispatcher.
5. **NPS público:** token HMAC (`INpsSurveyTokenService`), API anônima `/api/v1/public/nps/{token}`, filtro `NpsPublicTenantFilter` preenche `ITenantContext`.
6. **Última visita:** `ITutorVisitReadPort` agrega consultas e banho `Completed` (sem PDV nesta fatia).
7. **Relatórios:** `GET /api/v1/automations/nps/report` e `GET /api/v1/automations/reports/return-frequency`.

## Consequências

- Aceite: `Campaign_EnqueuedForInactiveSegment_WhenLastVisitOlderThan90Days`; `NpsResponse_RecordedAndReportable`; opt-out marketing em NPS.
- Campanha NPS pós-atendimento é criada já `Active` quando segmento `PostAppointment`.
- SMS permanece adiado (ADR-039).

## Confirmação no código

- [`CampaignDispatcher`](../../src/Modules/Automations/Application/Campaigns/CampaignDispatcher.cs)
- [`CampaignScanService`](../../src/Modules/Automations/Infrastructure/Campaigns/CampaignScanService.cs)
- [`NpsSurveyTokenService`](../../src/Modules/Automations/Infrastructure/Nps/NpsSurveyTokenService.cs)
- [`AutomationsPublicNpsEndpointExtensions`](../../src/API/Extensions/AutomationsPublicNpsEndpointExtensions.cs)

## Relacionados

- [ADR-038](./ADR-038-automations-outbox.md), [ADR-039](./ADR-039-lembretes-smtp-evolution.md)
- [`docs/diagramas/automations-campanhas-nps.mmd`](../diagramas/automations-campanhas-nps.mmd)
