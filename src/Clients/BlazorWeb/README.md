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

## Pré-requisitos (Linux / WSL)

Este projeto usa `<WasmBuildNative>true</WasmBuildNative>` para SQLite no browser ([ADR-014](../../docs/arquitetura/ADR-014-sqlite-local-clients.md)). No Linux/WSL:

```bash
dotnet workload install wasm-tools
sudo apt-get install -y libatomic1
```

Sem `libatomic1`, o link nativo (`emcc`) falha ao iniciar o Node do pack Emscripten (`libatomic.so.1: cannot open shared object file`).

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
- **CRM local:** `sysvet.db` + snapshot IndexedDB (`js/sqlite-db-storage.js`); `AddClientPersistence` em [`Program.cs`](./Program.cs).

Ver [ADR-012](../../docs/arquitetura/ADR-012-blazor-pwa-jwt.md), [ADR-014](../../docs/arquitetura/ADR-014-sqlite-local-clients.md).
