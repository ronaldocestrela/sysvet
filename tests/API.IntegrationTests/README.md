# `tests/API.IntegrationTests/` — Testes de Integração da API

Projeto de **testes de integração** que exercita endpoints HTTP da API de ponta a ponta (middlewares, auth, serialização, sync).

## Abordagem

- `WebApplicationFactory<Program>` sobe a API in-process
- `CoreDbContext` substituído por SQLite em arquivo temporário (por fixture de teste)
- Coleção xUnit `[Collection("IntegrationTests")]` para isolar estado compartilhado sensível

## PoC E2E sync (Fase 3.6)

| Arquivo | Descrição |
|---------|-----------|
| [`Sync/OfflineToCloudPocTests.cs`](Sync/OfflineToCloudPocTests.cs) | Worker real (`SyncBackgroundWorker`), métricas PoC |
| [`Sync/SyncPocHarness.cs`](Sync/SyncPocHarness.cs) | DI de teste: SQLite `:memory:` + HTTP autenticado |
| [`EndToEndSyncTests.cs`](EndToEndSyncTests.cs) | Push/pull HTTP direto (idempotência, FIFO tutor→pet) |

Documentação reproduzível: [`docs/arquitetura/sync-poc.md`](../../docs/arquitetura/sync-poc.md).

```bash
dotnet test tests/API.IntegrationTests/API.IntegrationTests.csproj --filter OfflineToCloudPocTests
```

## Dependências

| Pacote | Propósito |
|---|---|
| `Microsoft.AspNetCore.Mvc.Testing` | `WebApplicationFactory` |
| `FluentAssertions` | Asserções |
| `xUnit` | Framework de testes |

Referência de projeto: `Clients.Infrastructure` (outbox/worker nos testes PoC).
