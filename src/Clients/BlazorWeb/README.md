# `src/Clients/BlazorWeb/` — Blazor WebAssembly (PWA)

Host WASM do SysVet. **UI e layout** vêm de [`SharedUI`](../SharedUI/README.md); este projeto registra DI de plataforma (HTTP, auth, conectividade, SQLite offline).

## Arquivos principais

| Arquivo | Propósito |
|---|---|
| [`Program.cs`](./Program.cs) | `AddSharedUI()`, adapters `WebAuthState` / `WebNavigationService`, HttpClient + sync |
| [`App.razor`](./App.razor) | Router com `DefaultLayout = SharedUI.Layout.MainLayout` |
| [`wwwroot/index.html`](./wwwroot/index.html) | CSS: Bootstrap e tokens via `_content/SharedUI/` |
| [`wwwroot/css/app.css`](./wwwroot/css/app.css) | Apenas shell WASM (error UI, loading progress) |
| [`Services/`](./Services/) | Implementações host de contratos SharedUI |

Não há `Pages/` ou `Layout/` locais — rotas `@page` estão na SharedUI.

## PWA / offline

Manifest e service worker em `wwwroot/` (evolução na tarefa 3.2).
