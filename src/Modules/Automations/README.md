# `src/Modules/Automations/` — Módulo Automações

Fila durável de mensagens outbound (WhatsApp/e-mail), templates por canal, lembretes e worker com retry ([`docs/roadmap.md`](../../../docs/roadmap.md) § 8.1–8.2, [ADR-038](../../../docs/arquitetura/ADR-038-automations-outbox.md), [ADR-039](../../../docs/arquitetura/ADR-039-lembretes-smtp-evolution.md)).

## Status

> **Fases 8.1–8.3 concluídas.** Outbox SQL, lembretes, campanhas inativos, NPS pós-atendimento, opt-out canal + marketing, SMTP + Evolution (`Provider=Live`). SMS adiado.

## Escopo entregue

- Agregados `MessageTemplate`, `MessageJob`, `JobAttemptLog`, `TutorMessagingPreference`, `AutomationsSettings`
- Gatilhos: vacina D-7, consulta D-1, aniversário pet, retorno (`MedicalRecord.FollowUpOn`)
- Campanhas segmentadas, NPS tokenizado, relatório de retorno
- API `/api/v1/automations` (templates, jobs, campanhas, NPS) e `/api/v1/public/nps`
- Canal tutor `EnqueueingTutorNotificationChannel` (banho/tosa)

## Estrutura

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades, `BusinessHours`, renderer de tokens |
| [`Application/`](./Application/) | CQRS, `ReminderPlanner`, `MessageJobProcessor` |
| [`Infrastructure/`](./Infrastructure/) | EF, workers, gateways SMTP/Evolution, fontes de candidatos |

## Referências

- [`docs/functions.md`](../../../docs/functions.md) § Automação/Marketing
- [`docs/diagramas/automations-lembretes.mmd`](../../../docs/diagramas/automations-lembretes.mmd)
