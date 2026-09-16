# `src/Clients/MauiApp/` — Aplicação Mobile e Desktop (.NET MAUI)

Aplicação **multiplataforma** para Windows, macOS, iOS e Android, construída com **.NET MAUI** no modo **Blazor Hybrid**. Compartilha a lógica de UI com o `BlazorWeb` através da `SharedUI`.

## Status Atual

> **Scaffold funcional (Blazor Hybrid).** O projeto referencia `SharedUI` e `Clients.Infrastructure` (HTTP, SQLite offline, sync). No ambiente Linux/WSL o TFM está limitado a **`net10.0-android`** até que workloads MAUI adicionais estejam disponíveis.

## Solução principal

- O projeto está listado em [`SaaS_Veterinario.slnx`](../../../SaaS_Veterinario.slnx) com **build da solução desligado** (Debug/Release), para não exigir workload MAUI em CI/Linux.
- Para compilar o cliente MAUI quando o workload estiver instalado, use [`MauiApp.sln`](./MauiApp.sln) ou `dotnet build src/Clients/MauiApp/MauiApp.csproj`.

## O que há aqui

| Arquivo / Pasta | Função |
|---|---|
| `MauiApp.csproj` | Projeto MAUI; referencia `SharedUI` e `Clients.Infrastructure`. |
| `MauiProgram.cs` | Ponto de entrada; configura `MauiAppBuilder` com Blazor Hybrid. |
| `MainPage.xaml` / `.cs` | Página nativa que hospeda o `BlazorWebView`. |
| `Services/` | Conectividade, navegação, autenticação específicos do host MAUI. |
| `Resources/` | Fontes, ícones, splash e imagens por plataforma. |
| `Platforms/` | Entrypoints e código nativo por plataforma. |

## Diferencial da Arquitetura Hybrid

O MAUI não reescreve a UI — ele **reutiliza os mesmos componentes Razor** da `SharedUI` dentro de uma WebView nativa. Isso garante paridade visual entre web e desktop/mobile sem duplicação de código.
