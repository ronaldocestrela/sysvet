# `src/Modules/Core/` — Módulo Core

Módulo **transversal e base** do sistema: `Entity`/`AggregateRoot`, `ValueObject`, `Result<T>`, `ErrorCodes`, value objects de contato, entidades `Tutor`/`Pet` e pipeline CQRS compartilhado.

## Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/README.md) | Entidades, VOs, `IDomainEvent`, repositórios, `ErrorCodes` |
| [`Application/`](./Application/README.md) | MediatR, behaviors, commands/queries Tutor/Pet, políticas de autorização |
| [`Infrastructure/`](./Infrastructure/README.md) | DbContext, Identity/JWT, DI do pipeline, dispatch de domain events |

## Kernel CQRS (2.1)

- Marcadores: `ICommand`, `IQuery`, `IIdempotentCommand`
- Pipeline: Logging → Authorization → Validation → Idempotency → Transaction
- Commit: `IEnumerable<IUnitOfWork>` + dispatch de `IDomainEvent` após save
