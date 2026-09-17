# ADR-013: MAUI Blazor Hybrid — hosts finos, JWT e CI Windows

## Status
Accepted

## Data
2026-09-16

## Contexto

A Fase 3.3 exige cliente **MAUI Blazor Hybrid** (Android + Windows mínimo) reutilizando SharedUI e paridade JWT/CRM com Blazor WASM (ADR-012), sem entregar SQLite/sync offline (3.4–3.5). O repositório roda CI principal em Linux; build MAUI exige workload e, para Windows, runner `windows-latest`.

## Opções consideradas

1. **Duplicar AuthHandler e AuthorizeRouteView no MAUI** — rejeitado (viola ADR-011).
2. **Extrair auth HTTP e gate de rotas para SharedUI** — aceito; hosts registram apenas `ITokenStorage` e adapters de plataforma.
3. **Incluir MAUI no `SaaS_Veterinario.ci.slnf` (Linux)** — rejeitado; quebra restore sem workload Android/Windows.
4. **Job CI dedicado em Windows** — aceito para publish Windows + Android.

## Decisão

1. **SharedUI:** `ClientAuthState`, `ITokenStorage`, `AuthHandler`, `AuthTokenRefresher`, `AuthorizeRouteView` como SSOT entre WASM e MAUI.
2. **MAUI storage:** `MauiSecureTokenStorage` com chaves `sysvet_access_token` / `sysvet_refresh_token` (alinhado ao WASM).
3. **API URL:** `MauiApiConfiguration` + `appsettings.json` (MauiAsset); defaults por TFM — Android emulador `http://10.0.2.2:5222/`, Windows `https://localhost:7180/`. Cleartext HTTP no Android (Debug/manifest) para perfil HTTP local da API.
4. **TFMs:** `net10.0-android` sempre; `net10.0-windows10.0.19041.0` quando MSBuild roda em Windows (`IsOSPlatform('windows')`).
5. **Branding:** VetNexus (`#4A90E2`), `ApplicationId` `com.vetnexus.sysvet`; permissões `INTERNET`, `CAMERA` (opcional), `webcam` (Windows).
6. **CI:** job `maui-publish` em `windows-latest` — workload MAUI, publish Windows + Android, artefatos.

## Consequências

- Positivas: aceite 3.3; um fluxo de login/CRM idêntico ao PWA; testes Linux cobrem branding e auth SharedUI.
- Negativas: CI MAUI mais lento; cleartext Android deve ser revisado antes de produção.
- Futuro: 3.4 SQLite local; runtime permission câmera; assinatura de loja.

## Confirmação no código

- `src/Clients/SharedUI/Services/ClientAuthState.cs`, `Http/AuthHandler.cs`, `Routing/AuthorizeRouteView.razor`
- `src/Clients/MauiApp/MauiProgram.cs`, `Services/MauiSecureTokenStorage.cs`, `MauiApiConfiguration.cs`
- `tests/Clients.Tests/Maui/MauiBrandingTests.cs`
- `.github/workflows/ci.yml` — job `maui-publish`

## Relacionados

- [ADR-011](./ADR-011-sharedui-design-system.md), [ADR-012](./ADR-012-blazor-pwa-jwt.md)
- [roadmap.md](../roadmap.md) § 3.3
