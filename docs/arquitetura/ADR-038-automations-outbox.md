# ADR-038: Automations — outbox SQL e worker (Fase 8.1)

## Status
Accepted

## Data
2026-09-21

## Contexto

A Fase 8.1 exige módulo `Automations` com fila durável, worker, retry, log e templates por canal. O roadmap cita Azure Service Bus, RabbitMQ ou tabela outbox. Já existe outbox **client** para sync offline ([ADR-002](./ADR-002-estrategia-de-sync.md)); notificações ao tutor usam `ITutorNotificationChannel` com `NullTutorNotificationChannel` até esta fase ([ADR-031](./ADR-031-notificacoes-status-banho.md)). [ADR-006](./ADR-006-domain-events.md) prevê outbox servidor sem alterar contratos de domínio.

## Opções consideradas

1. **Broker gerenciado (Service Bus / RabbitMQ)** — escala horizontal; exige infra extra no MVP.
2. **Outbox SQL + `BackgroundService` no host API** — mesma stack EF/SQL Server/SQLite; jobs no schema do tenant ([ADR-003](./ADR-003-multi-tenancy.md)); alinhado ao monólito modular ([ADR-001](./ADR-001-monolito-modular.md)).

## Decisão

1. Criar `src/Modules/Automations/` (Domain/Application/Infrastructure) com agregados `MessageTemplate`, `MessageJob` (+ `JobAttemptLog`).
2. Fila = tabelas `MessageJobs` / `MessageTemplates` no `AutomationsDbContext`; worker `OutboxProcessor` (`BackgroundService`) no processo da API.
3. Retry exponencial (base 30s × 2^attempt); dead-letter após `MaxAttempts` (default 5); cada tentativa gera `JobAttemptLog`.
4. `IOutboundMessageSender` na Application; 8.1 usa `LoggingOutboundMessageSender` (sem Twilio/SendGrid — Fase 8.2).
5. `EnqueueingTutorNotificationChannel` implementa `ITutorNotificationChannel` e enfileira jobs via `EnqueueMessageJobCommand`.
6. Core registra `NullTutorNotificationChannel` com `TryAddSingleton`; Automations registra canal real depois (last-wins).

**Fora de 8.1:** opt-in/opt-out, horário comercial, provedores externos, varredura multi-schema (Platform 9.1).

## Consequências

- Aceite: job enfileirado → worker → sucesso com log; falha → retry → dead-letter.
- Distinto do outbox de sync client; não substitui ingestão síncrona de `POST /sync/push`.
- Worker processa o schema resolvido por `ITenantContext` no scope (dev/test: `dbo`).

## Confirmação no código

- [`AutomationsDbContext`](../../src/Modules/Automations/Infrastructure/Persistence/AutomationsDbContext.cs)
- [`OutboxProcessor`](../../src/Modules/Automations/Infrastructure/Workers/OutboxProcessor.cs)
- [`EnqueueingTutorNotificationChannel`](../../src/Modules/Automations/Infrastructure/Notifications/EnqueueingTutorNotificationChannel.cs)
- [`AutomationsEndpointExtensions`](../../src/API/Extensions/AutomationsEndpointExtensions.cs)

## Relacionados

- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-006](./ADR-006-domain-events.md), [ADR-031](./ADR-031-notificacoes-status-banho.md)
- [`docs/diagramas/automations-outbox.mmd`](../diagramas/automations-outbox.mmd)
