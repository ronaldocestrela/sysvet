# ADR 002: Estratégia de sincronização offline-first

## Status
Accepted (Outbox validado via PoC)

## Data
2026-08-18

## Contexto

Clínicas operam com internet instável. Clientes **Blazor WASM (PWA)** e **MAUI (Blazor Hybrid)** devem persistir localmente (SQLite) e sincronizar com o SQL Server central quando houver conectividade. Era necessário escolher um motor de sync bidirecional alinhado a TDD, Clean Architecture e EF Core sem poluir o schema de produção.

PoCs executadas em `tests/PoC.SyncTests` e projeto `src/PoC.Sync`.

## Opções consideradas

1. **Dotmim.Sync** — framework de sync delta servidor/cliente.
   - Prós: sync automatizada; conflitos por timestamp nativos.
   - Contras: servidor deve ser DB real (não SQLite in-memory para TDD); injeta tabelas `_tracking` / `_scope` no SQL Server; acoplamento forte ao motor de sync.

2. **Outbox Pattern + HTTP (push/pull)** — fila transacional local; worker envia comandos REST à API; pull por cursor/timestamp no servidor.
   - Prós: controle total; mesma transação EF para entidade + outbox; agnóstico a SQLite/SQL Server; testável com PoC (`Worker_Should_Process_OutboxMessage_And_Sync_To_CentralDb`).
   - Contras: implementação manual (worker, retry, idempotência, endpoints de sync).

3. **Event Sourcing completo** — log imutável de eventos como fonte da verdade.
   - Prós: auditoria e replay nativos.
   - Contras: complexidade e curva de aprendizado desproporcionais ao MVP CRM/PDV; **não adotado nesta fase**.

## Decisão

Adotar **Transactional Outbox** em `Clients.Infrastructure`: entidade `OutboxMessage` no `OfflineDbContext`; `SyncBackgroundWorker` (`BackgroundService`) nos clientes consome a fila e chama a API. **Rejeitar Dotmim.Sync** pelos bloqueios de PoC. **Não** usar Event Sourcing como estratégia principal agora.

Diagrama de sequência: [`docs/diagramas/sync-sequence.mmd`](../diagramas/sync-sequence.mmd).

### Regras operacionais (Fase 3.5)

- **Conflitos (CRM):** Last-Write-Wins por `UpdatedAt` (UTC) via `OccurredAt` nos commands de update; REST online continua com `RowVersion` / `409` quando aplicável.
- **Pull:** `PullChangesQuery` + `SyncChangeFeedReader` (tombstones com `IgnoreQueryFilters`); cliente aplica via `OfflineSyncPullApplier` com `SuppressOutbox`.
- **Push:** `PushSyncBatchCommand` (MediatR, stop-on-first-error); `OutboxMessage.Id` → `IdempotencyKey`; ingestão **síncrona** no `POST /api/v1/sync/push` (sem fila no servidor).
- **Ordem:** FIFO por `CreatedAt`; tutor antes de pet (FK).
- **Falhas no client:** backoff exponencial (`AttemptCount`, `NextRetryAt`); dead-letter em `OutboxMessage.Error` para falhas permanentes.
- **UI:** `ISyncConnectivity` / `SetSyncing` durante ciclo online; worker no client (Blazor WASM / MAUI).

## Consequências

- **Positivas:** alinhamento com TDD e repositório offline único; sem tabelas de tracking de terceiros no SQL Server.
- **Negativas:** worker WASM depende da aba aberta; snapshot IndexedDB copia o `.db` inteiro.
- **PoC E2E (3.6):** [`sync-poc.md`](./sync-poc.md) + `OfflineToCloudPocTests`.

## Confirmação no código

- [`OfflineDbContext`](../../src/Clients/Clients.Infrastructure/OfflineDbContext.cs) — outbox tutor/pet + delete; `SuppressOutbox`.
- [`OutboxMessage`](../../src/Clients/Clients.Infrastructure/Sync/OutboxMessage.cs) — retry/dead-letter.
- [`SyncBackgroundWorker`](../../src/Clients/Clients.Infrastructure/Sync/SyncBackgroundWorker.cs) — push/pull, backoff, `ISyncConnectivity`.
- [`PushSyncBatchCommand`](../../src/Modules/Core/Application/Sync/PushSyncBatchCommand.cs) / [`PullChangesQuery`](../../src/Modules/Core/Application/Sync/PullChangesQuery.cs) — ingestão CQRS.
- [`SyncEndpointExtensions`](../../src/API/Extensions/SyncEndpointExtensions.cs) — `/api/v1/sync/push|pull`.
- PoC histórica: [`tests/PoC.SyncTests/`](../../tests/PoC.SyncTests/); E2E HTTP: [`EndToEndSyncTests.cs`](../../tests/API.IntegrationTests/EndToEndSyncTests.cs); PoC 3.6: [`OfflineToCloudPocTests.cs`](../../tests/API.IntegrationTests/Sync/OfflineToCloudPocTests.cs).

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-003](./ADR-003-multi-tenancy.md) (`TenantId` no sync)
- [sync-sequence.mmd](../diagramas/sync-sequence.mmd)
- Cópia legada na raiz: [`ADR_002_Sincronizacao.md`](../../ADR_002_Sincronizacao.md) (aponta para este arquivo)
