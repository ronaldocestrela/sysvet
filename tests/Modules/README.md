# `tests/Modules/` — Testes dos Módulos de Negócio

Contém os projetos de **testes unitários** para cada módulo de negócio. A estrutura de subpastas dentro de cada projeto de teste espelha a estrutura do módulo correspondente em `src/Modules/`.

## Projetos de Teste

| Projeto | Módulo testado | Status |
|---|---|---|
| [`Core.Tests/`](./Core.Tests/README.md) | `src/Modules/Core` | 🟡 Em desenvolvimento — testes de Domain escritos |
| [`Veterinary.Tests/`](./Veterinary.Tests/) | `src/Modules/Veterinary` | 🔴 Placeholder — apenas `UnitTest1.cs` |
| [`Petshop.Tests/`](./Petshop.Tests/) | `src/Modules/Petshop` | 🔴 Placeholder |
| [`Sales.Tests/`](./Sales.Tests/) | `src/Modules/Sales` | 🔴 Placeholder |
| [`Inventory.Tests/`](./Inventory.Tests/) | `src/Modules/Inventory` | 🔴 Placeholder |
| [`Fiscal.Tests/`](./Fiscal.Tests/) | `src/Modules/Fiscal` | 🔴 Placeholder |

## Regra de Espelhamento

A pasta interna de cada projeto de teste deve espelhar a estrutura do projeto testado:

```
src/Modules/Core/Domain/Entities/Tutor.cs
       ↕ espelho
tests/Modules/Core.Tests/Domain/Entities/TutorTests.cs
```
