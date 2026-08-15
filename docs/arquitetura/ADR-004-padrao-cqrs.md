# ADR 004: Padrão CQRS e MediatR

## Status
Aceito

## Contexto
O monolito modular precisa processar regras de negócio complexas, com requisitos distintos para leitura e escrita, e necessita de um baixo acoplamento entre a camada de apresentação (API) e a de aplicação.

## Decisão
Adotaremos o padrão **CQRS (Command Query Responsibility Segregation)** implementado via **MediatR** em todos os módulos.
As requisições serão divididas estritamente entre `ICommand` (modificam estado) e `IQuery` (apenas leituras, sem efeitos colaterais).
Validações de comandos ocorrerão no pipeline do MediatR via FluentValidation (Behaviors).
Os resultados serão sempre tipados usando o pattern `Result<T>` em vez de lançar exceções para fluxo de controle.

## Consequências
- **Positivas**: Centralização de responsabilidades; Código testável e isolado (TDD friendly); Extensibilidade fácil via Pipeline Behaviors (Logging, Validation, Transaction).
- **Negativas**: Introduz boilerplate e mais classes para funcionalidades simples (CRUDs triviais podem parecer verbosos).
