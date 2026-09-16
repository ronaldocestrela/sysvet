# ADR 005: Mapeamento Result para HTTP na API

## Status
Aceito

## Contexto
Handlers CQRS retornam `Result` / `Result<T>` (ADR-004). Minimal APIs precisam traduzir falhas em status HTTP e RFC 7807 Problem Details, sem duplicar `if (result.IsFailure)` em cada rota.

## Decisão
1. **Helpers** em `API.Extensions.ResultExtensions`: `ToProblemDetails`, `ToHttpResult`, `ToCreatedAt`, com códigos derivados de `Error.Code` (`*NotFound` → 404, `*Conflict` → 409, `Unauthorized` / `Forbidden` → 401 / 403, validação → 400).
2. **Filtro** `ResultEndpointFilter` no grupo raiz de rotas da API para handlers que retornam `Result` diretamente (ex.: testes e rotas futuras).
3. **Endpoints async** que usam MediatR retornam explicitamente `(await mediator.Send(...)).ToHttpResult()` (ou `ToCreatedAt` / `Results.NoContent` quando o contrato exige 201/204), pois o retorno `Task<Result<T>>` não é convertido de forma confiável só pelo filtro.

## Consequências
- Respostas de erro consistentes entre módulos.
- `Program.cs` permanece composition root; regras de negócio não conhecem HTTP.
- Rotas com status especiais (201 Created, 204 NoContent) continuam explícitas no mapeamento.
