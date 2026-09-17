# `src/Clients/MauiApp/` — Aplicação Mobile e Desktop (.NET MAUI)

Aplicação **multiplataforma** (Android + Windows mínimo) no modo **Blazor Hybrid**. UI e fluxo CRM vêm de [`SharedUI`](../SharedUI/README.md); autenticação JWT compartilhada (ADR-013).

## Build

- **Linux/WSL:** `net10.0-android` apenas — exige workload MAUI Android.
- **Windows:** `net10.0-android` + `net10.0-windows10.0.19041.0`.

```bash
dotnet workload install maui
dotnet build src/Clients/MauiApp/MauiApp.csproj -f net10.0-android
# Windows:
dotnet build src/Clients/MauiApp/MauiApp.csproj -f net10.0-windows10.0.19041.0
```

O projeto está em [`SaaS_Veterinario.slnx`](../../../SaaS_Veterinario.slnx) com **build da solução desligado** no Linux. Use [`MauiApp.sln`](./MauiApp.sln) ou o `.csproj` diretamente.

CI: job `maui-publish` em `.github/workflows/ci.yml` (runner `windows-latest`).

## API local

| Plataforma | URL padrão | Notas |
|------------|------------|--------|
| Windows | `https://localhost:7180/` | Perfil `https` da API |
| Android emulador | `http://10.0.2.2:5222/` | HTTP; cleartext no manifest para Development |

Override opcional: `appsettings.json` (`ApiBaseUrl`) empacotado como MauiAsset.

Suba a API em Development e use o seed: `admin@sysvet.com` / `Password123!` ([`configuracao.md`](../../../docs/arquitetura/configuracao.md)).

## Estrutura

| Pasta / arquivo | Função |
|-----------------|--------|
| `MauiProgram.cs` | DI: SharedUI, JWT, HttpClient, `AddClientPersistence` (`AppDataDirectory/sysvet.db`) |
| `Services/MauiSecureTokenStorage.cs` | JWT em `SecureStorage` |
| `MainPage.xaml` | `BlazorWebView` + `Main.razor` |
| `Platforms/` | Android, Windows, iOS (stub futuro) |
