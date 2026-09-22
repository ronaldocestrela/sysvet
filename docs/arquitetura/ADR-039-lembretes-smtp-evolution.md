# ADR-039: Lembretes WhatsApp (Evolution API) e e-mail (SMTP) — Fase 8.2

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 8.2 exige gatilhos de lembrete (vacina D-7, consulta amanhã, aniversário do pet, retorno clínico), opt-out por canal, horário comercial e integração com provedores reais. A 8.1 entregou outbox SQL, worker e `LoggingOutboundMessageSender` ([ADR-038](./ADR-038-automations-outbox.md)). Alertas de vacina são projeção CQRS sem disparo ([ADR-016](./ADR-016-carteira-vacinacao.md)).

## Opções consideradas

1. **Twilio / SendGrid / Z-API** — citados no roadmap genérico; Evolution API já adotada pelo tenant para WhatsApp; SMTP genérico evita vendor lock-in de e-mail.
2. **Hangfire / Quartz para agendamento** — infra extra; candidatos são varridos por `ReminderScheduler` e enfileirados na mesma tabela `MessageJobs`.
3. **Preferências no Tutor (Core CRM)** — alteraria sync offline; preferências transacionais ficam no schema Automations.

## Decisão

1. **`ReminderScheduler`** (`BackgroundService`) consulta portas `IReminderCandidateSource` (adapters Veterinary + Core) e enfileira jobs com idempotência.
2. **Provedores:** `Automations:Provider` = `Fake` (CI/dev) | `Live`. Live: SMTP (`System.Net.Mail`) + `HttpClient` Evolution API v2 (`POST /message/sendText/{instance}`).
3. **SMS:** enum mantido; enqueue manual, scheduler e sender recusam `MessageChannel.Sms` até fatia futura.
4. **`TutorMessagingPreference`** e **`AutomationsSettings`** (horário comercial) no `AutomationsDbContext`.
5. **`MessageJob.DeferUntil`** fora da janela comercial sem incrementar tentativas.
6. **Retorno:** `MedicalRecord.FollowUpOn` (`DateOnly?`); lembrete D-1; skip se consulta ativa no mesmo dia local para o pet.
7. **Fuso:** `America/Sao_Paulo` para janelas de gatilho.

## Consequências

- Aceite: lembrete de vacina 7 dias antes; opt-out respeitado.
- Grooming WhatsApp (8.1) passa a respeitar opt-out e horário comercial no processor.
- Instância Evolution por tenant permanece Platform 9.x; credenciais globais em config até lá.

## Confirmação no código

- [`ReminderScheduler`](../../src/Modules/Automations/Infrastructure/Workers/ReminderScheduler.cs)
- [`ChannelOutboundMessageSender`](../../src/Modules/Automations/Infrastructure/Channels/ChannelOutboundMessageSender.cs)
- [`SmtpEmailGateway`](../../src/Modules/Automations/Infrastructure/Channels/SmtpEmailGateway.cs)
- [`EvolutionWhatsAppGateway`](../../src/Modules/Automations/Infrastructure/Channels/EvolutionWhatsAppGateway.cs)

## Relacionados

- [ADR-016](./ADR-016-carteira-vacinacao.md), [ADR-038](./ADR-038-automations-outbox.md)
- [`docs/diagramas/automations-lembretes.mmd`](../diagramas/automations-lembretes.mmd)
