# `src/Clients/` — Aplicativos Clientes

Contém todos os projetos de **interface com o usuário** — web, mobile/desktop, infra compartilhada e a biblioteca de componentes. CRM tutor/pet persiste em **SQLite local** (Fase 3.4); login e sync usam a API em `src/API/`.

## Estrutura Interna

| Subpasta | Plataforma | Propósito |
|---|---|---|
| [`BlazorWeb/`](./BlazorWeb/README.md) | Web (navegador) | Aplicação Blazor WebAssembly (PWA) |
| [`MauiApp/`](./MauiApp/README.md) | Windows / macOS / iOS / Android | Aplicação .NET MAUI (Blazor Hybrid) |
| [`Clients.Infrastructure/`](./Clients.Infrastructure/) | Multiplataforma | `OfflineDbContext`, `ITutorStore`/`IPetStore`, `ApiClient`, outbox |
| [`SharedUI/`](./SharedUI/README.md) | Multiplataforma | Razor Class Library com componentes reutilizáveis |

## Princípio de Reutilização

```
BlazorWeb ─┐
           ├──→ SharedUI (componentes Razor, lógica de estado da UI)
MauiApp ───┘
```

- **`SharedUI`** é a única fonte de verdade de componentes visuais. Nunca duplique um componente entre `BlazorWeb` e `MauiApp`.
- A lógica de estado, validações de formulário e regras de interface ficam em `SharedUI`, não nos clientes individuais.
- Telas CRM injetam `ITutorStore`/`IPetStore` (offline SQLite); outros fluxos usam serviços HTTP injetados, não `HttpClient` direto nos `.razor`.
