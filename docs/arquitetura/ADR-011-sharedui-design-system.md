# ADR-011: SharedUI design system (RCL)

## Status
Accepted

## Data
2026-09-16

## Contexto
Blazor WebAssembly e MAUI Blazor Hybrid devem compartilhar layout, tokens visuais e componentes base sem duplicar markup ou CSS ([`agents.md`](../agents.md) — Frontend e Reutilização). A Fase 3.1 exige paridade estrutural entre hosts antes de PWA, JWT e sync offline.

## Opções consideradas
1. **RCL SharedUI como SSOT** — tokens, layout e componentes em `src/Clients/SharedUI/`; hosts só registram adapters (`WebAuthState`, `MauiAuthState`, etc.).
2. **Duplicar layout em BlazorWeb e MauiApp** — rejeitado por violar reuso e roadmap.

## Decisão
Adotar **SharedUI** como Razor Class Library única: CSS tokens em `wwwroot/css/app.css`, Bootstrap servido pelo RCL (`_content/SharedUI/lib/bootstrap/`), componentes `DataGrid`, `FormField`, `Modal`, `Toast`, `LoadingState`, layouts `MainLayout` e `AuthLayout`, navegação estática via `AppNavItems` (filtro por permissões fica para 3.2). Toast global via `IToastService` registrado em `AddSharedUI()`.

## Consequências
- Positivas: um layout para WASM e MAUI; testes bUnit centralizados; hosts enxutos.
- Negativas: Bootstrap ainda é dependência utilitária (não design system puro).
- Futuro: tema escuro, paginação em `DataGrid`. Menu dinâmico via `/auth/me` entregue na 3.2 (ADR-012).

## Confirmação no código
- `src/Clients/SharedUI/` — RCL, tokens, componentes, `SharedUIServiceCollectionExtensions.AddSharedUI()`.
- `src/Clients/BlazorWeb/App.razor`, `src/Clients/MauiApp/Main.razor` — `DefaultLayout = SharedUI.Layout.MainLayout`.
- `tests/Clients.Tests/SharedUI/` — cobertura bUnit dos componentes e layout.

## Relacionados
- [ADR-009](./ADR-009-access-profiles.md) (menus por permissão — clientes)
- [roadmap.md](../roadmap.md) § 3.1
- [c4-containers.mmd](../diagramas/c4-containers.mmd)
