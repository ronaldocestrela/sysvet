# ADR 006: Eventos de domínio vs integração

## Status
Accepted

## Data
2026-09-16

## Contexto

O módulo Core passou a expor `IDomainEvent` e eventos em `AggregateRoot` (ADR-004, item 2.1). Módulos já publicavam `INotification` MediatR para integração (ex.: `OrderPaidEvent`). É necessário separar responsabilidades e o momento do dispatch.

## Decisão

1. **Eventos de domínio** (`IDomainEvent`): levantados dentro do agregado, coletados em memória e publicados **após** `SaveChangesAsync` bem-sucedido via `IDomainEventDispatcher` (adapter MediatR `DomainEventEnvelope` na Infrastructure).
2. **Eventos de integração** (`INotification` em Application): continuam explícitos nos handlers até existir outbox físico (ADR-002); não substituem domain events.
3. **Commit transacional**: `TransactionBehavior` persiste todos os `IUnitOfWork` registrados (`IEnumerable<IUnitOfWork>`); handlers não chamam `SaveChangesAsync` diretamente.

## Consequências

- Handlers permanecem finos; efeitos colaterais de persistência ficam no pipeline.
- Testes de domínio podem assertar eventos na coleção do agregado antes do dispatch.
- Outbox de **notificações** no servidor entregue em Automations 8.1 ([ADR-038](./ADR-038-automations-outbox.md)); distinto do outbox client de sync ([ADR-002](./ADR-002-estrategia-de-sync.md)).

## Confirmação no código

- [`Entity.cs`](../../src/Modules/Core/Domain/Entity.cs) — `AggregateRoot`, `Raise`, `ClearDomainEvents`
- [`TransactionBehavior.cs`](../../src/Modules/Core/Application/Behaviors/TransactionBehavior.cs)
- [`MediatRDomainEventDispatcher.cs`](../../src/Modules/Core/Infrastructure/Services/MediatRDomainEventDispatcher.cs)
- [`TutorRegisteredDomainEvent.cs`](../../src/Modules/Core/Domain/Events/TutorRegisteredDomainEvent.cs)

## Relacionados

- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-004](./ADR-004-padrao-cqrs.md)
