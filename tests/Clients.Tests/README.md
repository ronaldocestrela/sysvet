# `tests/Clients.Tests/` — Testes dos clientes

Testes de componentes Blazor (bUnit), adapters de host e infraestrutura offline compartilhada.

## Estrutura

```
Clients.Tests/
├── SharedUI/
│   ├── Components/     # DataGrid, FormField, Modal, Toast, LoadingState
│   ├── Layout/         # MainLayout, NavMenu, AuthLayout
│   └── Services/       # ToastService
├── BlazorWeb/          # PWA manifest
├── Maui/               # MauiBrandingTests (csproj, manifest, index.html)
├── SharedUI/Http/      # AuthHandler
├── SharedUI/Routing/   # AuthorizeRouteView
├── SharedUI/Navigation/  # MenuNavigation
├── Http/               # ApiClient
└── Offline*.cs         # SQLite / sync
```

## Execução

```bash
dotnet test tests/Clients.Tests/Clients.Tests.csproj
```

Pacotes: **bUnit**, **xUnit**, **FluentAssertions**, **MockHttp**.
