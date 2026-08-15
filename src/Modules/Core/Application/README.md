# `src/Modules/Core/Application/` — Camada de Aplicação do Módulo Core

Camada responsável por **orquestrar os casos de uso** do módulo Core, utilizando o padrão **CQRS** (Command Query Responsibility Segregation). Conhece o `Domain` mas não conhece a `Infrastructure`.

## Status Atual

> ⚠️ **Em estruturação.** Contém apenas o `Class1.cs` de placeholder gerado pelo template.

## O que virá aqui

Quando os casos de uso forem implementados, esta camada conterá:

```
Application/
├── Commands/
│   ├── CreateTutor/
│   │   ├── CreateTutorCommand.cs      ← Record com os dados de entrada
│   │   └── CreateTutorHandler.cs      ← Handler que orquestra Domain + Repositories
│   └── RegisterPet/
│       ├── RegisterPetCommand.cs
│       └── RegisterPetHandler.cs
├── Queries/
│   ├── GetTutorById/
│   │   ├── GetTutorByIdQuery.cs
│   │   └── GetTutorByIdHandler.cs
│   └── ListPetsByTutor/
│       ├── ListPetsByTutorQuery.cs
│       └── ListPetsByTutorHandler.cs
├── DTOs/
│   ├── TutorDto.cs                    ← Contrato de saída (sem expor entidades de domínio)
│   └── PetDto.cs
└── Interfaces/
    └── ITutorRepository.cs            ← Abstração de repositório definida na Application
```

## Regras desta Camada

- ❌ Sem referência a EF Core, Dapper ou qualquer ORM diretamente
- ❌ Sem retorno de entidades de domínio — use DTOs como contrato de saída
- ✅ Handlers dependem de interfaces de repositório (`ITutorRepository`), nunca da implementação concreta
- ✅ Todos os retornos de handlers devem usar `Result<T>`
