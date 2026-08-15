# `tests/` — Testes Automatizados

Esta pasta é o **espelho estrutural de `src/`**. Para cada projeto em `src/`, existe um projeto de testes correspondente aqui. Seguimos a metodologia **TDD**: nenhuma funcionalidade é implementada sem um teste falhando escrito antes.

## Estrutura

| Subpasta | O que testa | Tipo de teste |
|---|---|---|
| [`Modules/`](./Modules/README.md) | Módulos de negócio (Domain + Application) | Unitários |
| [`API.IntegrationTests/`](./API.IntegrationTests/README.md) | Endpoints HTTP da API | Integração |
| [`Clients.Tests/`](./Clients.Tests/README.md) | Lógica dos componentes Blazor | Unitários (bunit) |

## Frameworks e Bibliotecas

| Biblioteca | Propósito |
|---|---|
| `xUnit` | Framework de testes (AAA: Arrange, Act, Assert) |
| `Moq` ou `NSubstitute` | Mocking de dependências (repositórios, serviços externos) |
| `FluentAssertions` | Asserções mais legíveis (futuro) |
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` para testes de integração |
| `bunit` | Testes de componentes Razor (futuro) |

## Convenção de Nomenclatura

```
[Método]_[Cenário]_[ResultadoEsperado]()

Exemplo:
Create_WithEmptyName_ShouldReturnFailureResult()
AddPet_WithNullPet_ShouldReturnFailureWithNullPetError()
```

## Regras

- ✅ Todo teste deve seguir o padrão **AAA** (Arrange / Act / Assert) com comentários explícitos
- ✅ Testes unitários **não acessam banco de dados** — dependências são mockadas
- ✅ A cobertura deve focar intensamente em `Domain` e `Application`
- ❌ Nunca use `Thread.Sleep` ou delays fixos nos testes
