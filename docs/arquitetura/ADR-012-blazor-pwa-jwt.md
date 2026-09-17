# ADR-012: Blazor WASM PWA, JWT no cliente e CORS

## Status
Accepted

## Data
2026-09-16

## Contexto

A Fase 3.2 exige Blazor WebAssembly instalável como PWA, HttpClient autenticado contra a API, telas CRM (login, tutores, pets) e indicador de conectividade, conforme [`agents.md`](../agents.md) e [`roadmap.md`](../roadmap.md). O host WASM deve permanecer fino; UI compartilhada vive em SharedUI (ADR-011).

## Opções consideradas

1. **Cookies httpOnly para JWT** — rejeitado para WASM puro (sem endpoint BFF no escopo 3.2).
2. **localStorage + refresh via HttpClient dedicado** — aceito; alinhado a SPA/PWA comuns; risco XSS documentado.
3. **Cache de API no service worker** — rejeitado; offline 3.2 limita-se a assets estáticos e shell SPA.

## Decisão

1. **PWA:** template Blazor (`service-worker.js` stub em dev; `service-worker.published.js` + `service-worker-assets.js` no publish). Manifest com ícones 192/512, `standalone`, `theme_color`.
2. **JWT:** `IAuthState` estendido (access + refresh, menus de `/me`); persistência WASM em `localStorage` via `ITokenStorage`; MAUI em `SecureStorage`.
3. **HTTP:** `ApiClient` em `Clients.Infrastructure` com DTOs espelho (sem referência a `Core.Application`); `AuthHandler` anexa Bearer, ignora login/refresh, tenta refresh uma vez em 401.
4. **CORS:** policy `BlazorWeb` com origens em `Cors:AllowedOrigins`; `UseCors` antes de autenticação.
5. **Config:** `ApiBaseUrl` em `wwwroot/appsettings*.json` do BlazorWeb.
6. **Dev seed:** `DevelopmentAdminUserSeeder` (Development only) — `admin@sysvet.com` / `Password123!`, tenant fixo `11111111-1111-1111-1111-111111111111`.
7. **NavMenu:** filtrado por chaves `Menus` de `GET /api/v1/auth/me` via `MenuNavigation`.
8. **Conectividade:** `ConnectivityStatus` (Online / Offline / Syncing reservado para 3.5); banner descreve cache PWA; **CRM local** entregue na 3.4 ([ADR-014](./ADR-014-sqlite-local-clients.md)).

## Consequências

- Positivas: aceite 3.2 sem SQLite/sync; paridade MAUI entregue na 3.3 (ADR-013) via `ClientAuthState` e handlers SharedUI.
- Negativas: refresh token em localStorage é sensível a XSS; SW offline não cobre dados CRM.
- Futuro: 3.5 sync alimenta `Syncing`; BFF opcional para cookies. SQLite CRM: ADR-014.

## Confirmação no código

- `src/Clients/BlazorWeb/` — PWA; auth via SharedUI (`ClientAuthState`, `AuthHandler`, `AuthorizeRouteView`).
- `src/Clients/SharedUI/Pages/Login.razor`, `Tutors.razor`, `Pets.razor`.
- `src/Clients/Clients.Infrastructure/Http/ApiClient.cs`, DTOs CRM/auth.
- `src/API/Extensions/CorsExtensions.cs`.
- `Core.Infrastructure/Persistence/Seeding/DevelopmentAdminUserSeeder.cs`.
- `tests/Clients.Tests/BlazorWeb/`, `tests/API.IntegrationTests/CorsTests.cs`.

## Relacionados

- [ADR-007](./ADR-007-jwt-rbac.md), [ADR-011](./ADR-011-sharedui-design-system.md), [ADR-009](./ADR-009-access-profiles.md)
- [configuracao.md](./configuracao.md)
