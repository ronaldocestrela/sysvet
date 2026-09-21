# ADR-031: Notificações de status (banho e tosa)

## Status
Accepted

## Data
2026-09-21

## Contexto
A fase 6.7 exige avisar o tutor sobre início e “pronto para retirada” do banho, com tempo real no backoffice. O módulo Automations (8.1) ainda não existe; o ADR-030 reservou eventos de domínio para esta fase.

## Opções consideradas
1. **Criar Automations + fila agora** — escopo da Fase 8; atrasa 6.7.
2. **SignalR + porta `ITutorNotificationChannel`** — tempo real imediato; WhatsApp/SMS entra quando 8.1 registrar o canal.

## Decisão
- Novo status `ReadyForPickup` e comando `MarkGroomingReady`; `Complete` aceita `InProgress` (ready implícito) ou `ReadyForPickup`.
- Domain events → `GroomingStatusChangedEvent` (Core) via `DomainEventEnvelope`; handlers não lançam após commit.
- Hub SignalR `/hubs/grooming-status`, grupo `tenant-{TenantId}`, JWT em query `access_token`.
- `ITutorNotificationChannel` com `NullTutorNotificationChannel` (`IsEnabled = false`); Automations substitui o registro.

## Consequências
- Tutor só recebe mensagem quando um canal real estiver ativo; aceite usa fake no teste de integração.
- Complete direto de `InProgress` dispara ready implícito (notificação de pronto no atalho “Concluir”).

## Confirmação no código
- [`GroomingStatusChangedEvent`](../../src/Modules/Core/Application/IntegrationEvents/GroomingStatusChangedEvent.cs)
- [`ITutorNotificationChannel`](../../src/Modules/Core/Application/Notifications/ITutorNotificationChannel.cs)
- [`GroomingStatusHub`](../../src/API/Hubs/GroomingStatusHub.cs)
- [`POST .../ready`](../../src/API/Extensions/PetshopEndpointExtensions.cs)

## Relacionados
- [ADR-030](./ADR-030-estetica-banho-tosa.md), [ADR-006](./ADR-006-domain-events.md)
- [`docs/roadmap.md`](../roadmap.md) § 6.7
