# ADR 004: Padrão CQRS e MediatR

## Status
Accepted

## Data
2026-08-22

## Contexto

O monólito modular (ADR-001) expõe Minimal APIs que não devem conter regras de negócio. Leituras e escritas têm requisitos diferentes (validação, transação, idempotência). A camada Application precisa ser testável com TDD ([`docs/agents.md`](../agents.md)).

## Opções consideradas

1. **Serviços de aplicação “gordos”** — controllers/endpoints chamam services diretamente.
   - Prós: menos classes.
   - Contras: acoplamento à API; difícil compor cross-cutting.

2. **CQRS com MediatR** — commands/queries + pipeline behaviors.
   - Prós: handlers isolados; behaviors reutilizáveis (logging, validação, transação, idempotência).
   - Contras: boilerplate em CRUDs simples.

3. **CQRS com bus customizado** — sem MediatR.
   - Prós: controle total.
   - Contras: reinventar pipeline; descartado.

## Decisão

Adotar **CQRS via MediatR** em todos os módulos: `ICommand` / `IQuery` (marcadores em Core.Application), handlers por feature, **FluentValidation** no pipeline, retorno **`Result<T>`** para fluxo de negócio (sem exceções para validação esperada).

Behaviors registrados no Core: `LoggingBehavior`, `ValidationBehavior`, `IdempotencyBehavior`, `TransactionBehavior`.

## Consequências

- **Positivas:** testes de handlers e validators isolados; extensão por behaviors.
- **Negativas:** mais arquivos por use case.
- **Integração entre módulos:** eventos de integração via `INotification` / `INotificationHandler` (ex. `OrderPaidEvent`).

## Confirmação no código

- Registro MediatR: [`Core.Infrastructure/DependencyInjection.cs`](../../src/Modules/Core/Infrastructure/DependencyInjection.cs) — `RegisterServicesFromAssembly(typeof(CreatePetCommand).Assembly)` + behaviors.
- Marcadores: [`ICommand`](../../src/Modules/Core/Application/Messaging/ICommand.cs), queries em `Core.Application`.
- Handlers exemplo: `CreatePetCommandHandler`, `ScheduleAppointmentCommandHandler`, `RegisterProductCommandHandler`.
- Módulos satélites registram assembly próprio em `AddVeterinaryModule`, `AddInventoryModule`, `AddSalesModule`, etc.
- HTTP: ver [ADR-005](./ADR-005-result-http.md).

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-005](./ADR-005-result-http.md)
- [c4-api-components.mmd](../diagramas/c4-api-components.mmd)
