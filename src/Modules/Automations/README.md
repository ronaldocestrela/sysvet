# `src/Modules/Automations/` — Módulo Automações

Fila durável de mensagens outbound (WhatsApp/SMS/e-mail), templates por canal e worker com retry ([`docs/roadmap.md`](../../../docs/roadmap.md) § 8.1, [ADR-038](../../../docs/arquitetura/ADR-038-automations-outbox.md)).

## Status

> **Fase 8.1 concluída.** Outbox SQL, `OutboxProcessor`, templates, API `/api/v1/automations`, canal tutor `EnqueueingTutorNotificationChannel`. Provedores externos na 8.2.

## Escopo entregue (8.1)

- Agregados `MessageTemplate`, `MessageJob`, `JobAttemptLog`
- Worker in-process + `LoggingOutboundMessageSender`
- Substitui `NullTutorNotificationChannel` quando o módulo está registrado
- Permissões `Automations.Read` / `Automations.Write`; menu `automations`

## Estrutura

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/) | Entidades, renderer de tokens, repositórios |
| [`Application/`](./Application/) | CQRS, `MessageJobProcessor`, porta `IOutboundMessageSender` |
| [`Infrastructure/`](./Infrastructure/) | EF, worker, seed de templates grooming, DI |

## Referências

- [`docs/functions.md`](../../../docs/functions.md) § Automação/Marketing
- [`docs/arquitetura/ADR-031-notificacoes-status-banho.md`](../../../docs/arquitetura/ADR-031-notificacoes-status-banho.md)
