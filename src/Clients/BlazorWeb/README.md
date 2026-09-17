# `src/Clients/BlazorWeb/` — Blazor WebAssembly (PWA)

Host WASM do SysVet. **UI e layout** vêm de [`SharedUI`](../SharedUI/README.md); este projeto registra DI de plataforma (HTTP, auth, conectividade).

## Arquivos principais

| Arquivo | Propósito |
|---|---|
| [`Program.cs`](./Program.cs) | `ApiBaseUrl`, `AddSharedUI()`, JWT (`ClientAuthState`, SharedUI `AuthHandler`, refresh), HttpClient `"API"` / `"Auth"` |
| [`App.razor`](./App.razor) | Router + SharedUI [`AuthorizeRouteView`](../SharedUI/Routing/AuthorizeRouteView.razor) |
| [`wwwroot/appsettings*.json`](./wwwroot/appsettings.json) | URL da API (`https://localhost:7180/` em dev) |
| [`wwwroot/manifest.json`](./wwwroot/manifest.json) | PWA manifest (192/512) |
| [`Services/`](./Services/) | Auth, tokens (`localStorage`), conectividade |

Rotas `@page` estão na SharedUI.

## Executar (dev)

```bash
# Terminal 1 — API
dotnet run --project src/API/API.csproj --launch-profile https

# Terminal 2 — Blazor
dotnet run --project src/Clients/BlazorWeb/BlazorWeb.csproj --launch-profile https
```

Login dev (seed automático): `admin@sysvet.com` / `Password123!`

## PWA / offline

- **Dev:** `service-worker.js` é stub (sem cache).
- **Publish:** `dotnet publish` gera SW com cache de assets; validar instalação servindo `artifacts/.../wwwroot/`.

Ver [ADR-012](../../docs/arquitetura/ADR-012-blazor-pwa-jwt.md).
