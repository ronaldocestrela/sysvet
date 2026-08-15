# `src/Modules/Core/` — Módulo Core

Módulo **transversal e base** do sistema. Define os blocos fundamentais utilizados por todos os outros módulos: a classe base `Entity`, o `AggregateRoot`, o padrão `Result<T>`, os Value Objects de dados de contato e as entidades centrais `Tutor` e `Pet`.

> **Importante:** Este módulo não representa um domínio de negócio específico, mas sim a **linguagem comum** da aplicação. Qualquer tipo que precise ser compartilhado entre módulos deve ser definido aqui.

## Camadas

| Pasta | Responsabilidade |
|---|---|
| [`Domain/`](./Domain/README.md) | Entidades de domínio, Value Objects, classe base Entity, padrão Result |
| [`Application/`](./Application/README.md) | Handlers CQRS, DTOs e validações (ainda em estruturação) |
| [`Infrastructure/`](./Infrastructure/README.md) | DbContext do módulo Core, repositórios e configurações EF Core (ainda em estruturação) |

## O que já existe

- ✅ `Entity` e `AggregateRoot` — bases para todas as entidades
- ✅ `Result<T>` e `Error` — padrão de retorno sem exceções
- ✅ Value Objects: `Cpf`, `Email`, `Phone`
- ✅ Entidades: `Tutor` (Aggregate Root), `Pet`
- ✅ Enumerações: `PetSpecies`, `PetSex`
