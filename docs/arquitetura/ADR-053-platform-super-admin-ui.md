# ADR-053: UI Super Admin — client PlatformWeb (9.8)

## Status
Accepted

## Data
2026-09-23

## Contexto

As fases 9.2–9.7 expõem o backoffice VetNexus via `/api/v1/platform/*` (policy `PlatformAdmin`, role `SuperAdmin`). O roadmap 9.8 exige UI Blazor para operadores gerenciarem tenants sem SQL. O backoffice §5 (MRR, churn, LTV) permanece na Fase 10.2.

## Opções consideradas

1. **Área `/platform` no BlazorWeb staff** — compartilha SharedUI, rotas clínicas e sessão com `TenantId`; risco de confusão e vazamento de navegação; rejeitado (mesmo racional de ADR-041).
2. **Client WASM dedicado `PlatformWeb`** — aceito; espelha `TutorPortalWeb` (PWA fino, tokens isolados, sem SQLite/sync).
3. **Impersonation abre BlazorWeb automaticamente** — rejeitado nesta fatia; a UI exibe o JWT curto e encerra sessão via API; o operador usa o token manualmente no app clínico se necessário.

## Decisão

1. **`src/Clients/PlatformWeb/`** — PWA Blazor WebAssembly; `localStorage` com prefixo `platform_*`; portas dev `https://localhost:7282` / `http://localhost:5280`.
2. **`IPlatformAdminApi`** em `Clients.Infrastructure` — DTOs espelho e chamadas à API existente; sem novos handlers CQRS.
3. **`PlatformAuthState`** — estende o fluxo de login com roles de `/api/v1/auth/me`; apenas `SuperAdmin` permanece autenticado.
4. **Escopo UI:** tenants (onboarding, status, filiais), catálogo planos/add-ons (leitura), assinatura/flags, billing/cupons, health, API keys, impersonation, auditoria (login/change/impersonation).
5. **Fora do escopo:** BI SaaS global (10.2); criação de planos/add-ons na UI (catálogo seed 9.3).

## Consequências

- Aceite roadmap: operador VetNexus gerencia tenant sem acesso SQL direto.
- CORS: origens PlatformWeb em `Cors:AllowedOrigins`.
- Living docs: `agents.md`, `structure.md`, `backoffice.md`, `roadmap.md` §9.8, `STATUS_DO_PROJETO.md`, `configuracao.md`.

## Confirmação no código

- [`PlatformWeb/Program.cs`](../../src/Clients/PlatformWeb/Program.cs)
- [`PlatformAdminApiService`](../../src/Clients/Clients.Infrastructure/Platform/PlatformAdminApiService.cs)
- [`PlatformEndpointsExtensions`](../../src/API/Extensions/PlatformEndpointsExtensions.cs)

## Relacionados

- [ADR-041](./ADR-041-tutor-portal-base.md), [ADR-047](./ADR-047-onboarding-tenants-filiais.md)–[ADR-052](./ADR-052-auditoria-api-keys-health.md)
- [`docs/backoffice.md`](../backoffice.md)
