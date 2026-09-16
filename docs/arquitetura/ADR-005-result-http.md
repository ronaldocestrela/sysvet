# ADR 005: Mapeamento Result para HTTP na API

## Status
Accepted

## Data
2026-09-01

## Contexto

Handlers CQRS retornam `Result` / `Result<T>` (ADR-004). Minimal APIs precisam traduzir falhas em status HTTP e RFC 7807 Problem Details, sem duplicar `if (result.IsFailure)` em cada rota.

## Opções consideradas

1. **Exceções para tudo** — filter global de exceções.
   - Contras: viola [`agents.md`](../agents.md) para fluxo de negócio.

2. **Mapeamento manual por endpoint** — repetir switch de códigos.
   - Contras: inconsistência entre módulos.

3. **Extensions + filtro de endpoint** — `ToHttpResult` / `ToProblemDetails` + `ResultEndpointFilter` no grupo raiz.
   - Prós: contrato único; MediatR explícito onde `Task<Result<T>>` não passa pelo filtro.

## Decisão

1. **Helpers** em [`ResultExtensions`](../../src/API/Extensions/ResultExtensions.cs): `ToProblemDetails`, `ToHttpResult`, `ToCreatedAt`; códigos derivados de `Error.Code` (`*NotFound` → 404, `*Conflict` → 409, `Unauthorized` / `Forbidden` → 401 / 403, validação → 400).
2. **Filtro** [`ResultEndpointFilter`](../../src/API/Middlewares/ResultEndpointFilter.cs) no grupo raiz em [`Program.cs`](../../src/API/Program.cs).
3. **Endpoints MediatR** retornam explicitamente `(await mediator.Send(...)).ToHttpResult()` (ou `ToCreatedAt` / `Results.NoContent` para 201/204).

## Consequências

- Respostas de erro consistentes entre módulos.
- Corpo RFC 7807 inclui `errors[]` (`Code`, `Message`) e `correlationId` quando disponível — ver [configuracao.md](./configuracao.md#4-problem-details-resultfailure).
- `Program.cs` permanece composition root; domínio não conhece HTTP.
- Rotas com status especiais (201, 204) permanecem explícitas no mapeamento.

## Confirmação no código

- [`ResultExtensions.cs`](../../src/API/Extensions/ResultExtensions.cs), [`ResultEndpointFilter.cs`](../../src/API/Middlewares/ResultEndpointFilter.cs).
- [`Program.cs`](../../src/API/Program.cs) — `MapGroup(string.Empty).AddEndpointFilter<ResultEndpointFilter>()`.
- Testes: `ResultExtensionsTests`, `ResultEndpointFilterTests` em `tests/API.IntegrationTests/`.
- Tipo de domínio: [`Result.cs`](../../src/Modules/Core/Domain/Result.cs).

## Relacionados

- [ADR-004](./ADR-004-padrao-cqrs.md)
- [configuracao.md](./configuracao.md) — Problem Details / exception handler
