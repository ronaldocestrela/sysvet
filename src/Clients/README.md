# `src/Clients/` — Aplicativos Clientes

Contém todos os projetos de **interface com o usuário** — web, mobile/desktop e a biblioteca de componentes compartilhados. Todos os clientes consomem a API em `src/API/` via HTTP.

## Estrutura Interna

| Subpasta | Plataforma | Propósito |
|---|---|---|
| [`BlazorWeb/`](./BlazorWeb/README.md) | Web (navegador) | Aplicação Blazor WebAssembly (PWA) |
| [`MauiApp/`](./MauiApp/README.md) | Windows / macOS / iOS / Android | Aplicação .NET MAUI (Blazor Hybrid) |
| [`SharedUI/`](./SharedUI/README.md) | Multiplataforma | Razor Class Library com componentes reutilizáveis |

## Princípio de Reutilização

```
BlazorWeb ─┐
           ├──→ SharedUI (componentes Razor, lógica de estado da UI)
MauiApp ───┘
```

- **`SharedUI`** é a única fonte de verdade de componentes visuais. Nunca duplique um componente entre `BlazorWeb` e `MauiApp`.
- A lógica de estado, validações de formulário e regras de interface ficam em `SharedUI`, não nos clientes individuais.
- Chamadas HTTP à API são feitas via serviços injetados, nunca diretamente nos componentes `.razor`.
