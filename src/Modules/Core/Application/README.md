# `src/Modules/Core/Application/` — Camada de Aplicação do Módulo Core

Orquestra casos de uso com **CQRS (MediatR)**, retornando `Result<T>`.

## Estrutura

```
Application/
├── Authorization/          ← nomes de políticas (ClinicStaff, Cashier, …)
├── Behaviors/              ← pipeline MediatR
├── Messaging/              ← ICommand, IQuery
├── Common/                 ← IIdempotentCommand, ICurrentUser, IDomainEventDispatcher
├── Tutors/                 ← Commands, Queries, TutorMappings
├── Pets/                   ← Commands, Queries, PetMappings
└── IntegrationEvents/      ← OrderPaidEvent, DomainEventEnvelope
```

## Regras

- Handlers dependem de interfaces de repositório; não chamam `SaveChangesAsync` (responsabilidade do `TransactionBehavior`).
- Commands/queries sensíveis usam `[AuthorizeRequest(AuthorizationPolicies.*)]`.
- DTOs de saída via mappers estáticos por feature.
