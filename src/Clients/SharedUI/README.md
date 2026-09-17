# `src/Clients/SharedUI/` — Design system (RCL)

Razor Class Library compartilhada por **BlazorWeb** e **MauiApp**. Única fonte de verdade para layout, tokens CSS e componentes base (ADR-011).

## Estrutura

| Pasta | Conteúdo |
|---|---|
| [`Components/`](./Components/) | `DataGrid`, `FormField`, `Modal`, `Toast`, `LoadingState`, banners de conectividade |
| [`Layout/`](./Layout/) | `MainLayout`, `AuthLayout`, `NavMenu` |
| [`Navigation/`](./Navigation/) | `AppRoutes`, `AppNavItems` (rotas sem magic strings) |
| [`Services/`](./Services/) | `IAuthState`, `ClientAuthState`, `ITokenStorage`, `INavigationService`, `IToastService` |
| [`Http/`](./Http/) | `AuthHandler`, `AuthTokenRefresher` |
| [`Routing/`](./Routing/) | `AuthorizeRouteView` |
| [`DependencyInjection/`](./DependencyInjection/) | `AddSharedUI()` |
| [`wwwroot/css/app.css`](./wwwroot/css/app.css) | Tokens VetNexus (`:root`) |
| [`wwwroot/lib/bootstrap/`](./wwwroot/lib/bootstrap/) | Bootstrap servido via `_content/SharedUI/` |

## Uso nos hosts

```csharp
builder.Services.AddSharedUI();
builder.Services.AddSingleton<ITokenStorage, WebTokenStorage>(); // ou MauiSecureTokenStorage
builder.Services.AddSingleton<IAuthState, ClientAuthState>();
builder.Services.AddTransient<SharedUI.Http.AuthHandler>();
builder.Services.AddSingleton<INavigationService, WebNavigationService>();
```

Router: `DefaultLayout="@typeof(SharedUI.Layout.MainLayout)"` e `AdditionalAssemblies` apontando para este assembly.

## Testes

`tests/Clients.Tests/SharedUI/` — bUnit para componentes e layout.

> **Regra:** Nenhum componente visual reutilizável deve existir apenas em `BlazorWeb/` ou `MauiApp/`.
