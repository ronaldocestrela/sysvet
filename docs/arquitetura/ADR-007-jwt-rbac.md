# ADR 007: JWT Bearer, refresh tokens e RBAC por policies

## Status
Accepted

## Data
2026-09-16

## Contexto

A Fase 2.3 exige autenticação clínica com ASP.NET Core Identity, JWT Bearer e RBAC fixo por roles (`Admin`, `Veterinarian`, `Receptionist`, `Cashier`), alinhado a [`agents.md`](../agents.md) (CQRS, `Result<T>`, Clean Architecture). Perfis customizáveis (matriz permissão × recurso) ficam na tarefa 2.5.

Multi-tenancy (ADR-003) exige `TenantId` no JWT e `TenantResolutionMiddleware` após `UseAuthentication()` (ADR-046).

## Opções consideradas

1. **Auth inline nos Minimal APIs** — lógica em `AuthEndpointsExtensions` com `UserManager` direto.
   - Contras: viola ADR-004; difícil testar handlers.

2. **Auth via CQRS + serviços na Infrastructure** — commands `Login`, `Refresh`, `Register` (dev), query `GetCurrentUser`; Identity atrás de `IIdentityService`.
   - Prós: TDD na Application; endpoints finos; `Result<T>` consistente.

3. **Refresh token em `SecurityStamp`**
   - Contras: invalida stamp do Identity; rejeitado.

## Decisão

1. **JWT access token** emitido por `IAccessTokenIssuer` (`JwtAccessTokenIssuer`): claims `sub`, `nameidentifier`, `email`, `TenantId`, `role`, `jti`; validação via `JwtBearerOptionsConfiguration` (`MapInboundClaims = false` para claims customizados).
2. **Refresh token** persistido em `UserRefreshTokens` (hash SHA-256, expiração `JwtSettings:RefreshExpiryDays`, rotação no `RefreshTokenCommand`).
3. **RBAC:** policies em `AuthorizationPolicies` + `[AuthorizeRequest]` no MediatR; HTTP usa `RequireAuthorization(policy)` onde aplicável (ex.: `ClinicStaff` em tutors/pets).
4. **`POST /api/v1/auth/register`** apenas em `Development` (handler + rota condicional).
5. **`AppUser.TenantId`** não é sobrescrito por `SetTenantIdOnSave` (apenas shadow `TenantId` de entidades CRM).

## Consequências

- OpenAPI/Scalar documentam esquema Bearer (`OpenApiBearerSecurityTransformer`).
- Logout/revogação em massa e role `SuperAdmin` permanecem fora do escopo 2.3; role **`Tutor`** e portal do tutor foram entregues na Fase 8.4 ([ADR-041](./ADR-041-tutor-portal-base.md)).

## Confirmação no código

- Application: `Core.Application/Auth/` (commands, handlers, DTOs).
- Infrastructure: `IdentityService`, `RefreshTokenStore`, `JwtAccessTokenIssuer`, migration `AddUserRefreshTokens`.
- API: [`AuthEndpointsExtensions`](../../src/API/Extensions/AuthEndpointsExtensions.cs).
- Testes: `Core.Tests/Application/Auth/*`, `API.IntegrationTests/AuthEndpointsTests.cs`, `AuthorizationTests.cs`.

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-004](./ADR-004-padrao-cqrs.md), [ADR-005](./ADR-005-result-http.md), [ADR-009](./ADR-009-access-profiles.md) (perfis customizáveis e matriz de permissões)
- [configuracao.md](./configuracao.md)
