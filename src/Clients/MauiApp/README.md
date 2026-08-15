# `src/Clients/MauiApp/` — Aplicação Mobile e Desktop (.NET MAUI)

Aplicação **multiplataforma** para Windows, macOS, iOS e Android, construída com **.NET MAUI** no modo **Blazor Hybrid**. Compartilha a lógica de UI com o `BlazorWeb` através da `SharedUI`.

## Status Atual

> ⚠️ **Esta pasta ainda está vazia** (apenas `.gitkeep`). O projeto MAUI ainda não foi inicializado.

## O que virá aqui

Quando implementado, conterá:

| Arquivo / Pasta | O que será / Para que servirá |
|---|---|
| `MauiApp.csproj` | Projeto MAUI com targets para todas as plataformas. Referenciará `SharedUI`. |
| `MauiProgram.cs` | Ponto de entrada. Configura `MauiAppBuilder` com Blazor Hybrid (`AddMauiBlazorWebView`). |
| `MainPage.xaml` / `.cs` | Página nativa MAUI que hospeda o `BlazorWebView` com os componentes de `SharedUI`. |
| `Resources/` | Fontes, ícones, splash screen e imagens específicas de cada plataforma. |
| `Platforms/` | Código nativo por plataforma (permissões Android, entrypoint iOS, etc.). |
| `Data/` | Contexto SQLite local para modo offline, espelhando o schema do servidor. |

## Diferencial da Arquitetura Hybrid

O MAUI não reescreve a UI — ele **reutiliza os mesmos componentes Razor** da `SharedUI` dentro de uma WebView nativa. Isso garante paridade visual entre web e desktop/mobile sem duplicação de código.
