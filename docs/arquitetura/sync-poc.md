# PoC E2E offline → nuvem (Fase 3.6)

Documentação reproduzível da prova de conceito **SQLite local → `SyncBackgroundWorker` → API SysVet**. Complementa [ADR-002](./ADR-002-estrategia-de-sync.md) (decisão Outbox) e [ADR-014](./ADR-014-sqlite-local-clients.md) (SQLite nos clients).

## Propósito

| Artefato | Papel |
|----------|--------|
| `tests/PoC.SyncTests`, `src/PoC.Sync` | PoC **histórica** (Dotmim vs Outbox); decisão já tomada |
| `tests/API.IntegrationTests/EndToEndSyncTests.cs` | E2E HTTP push/pull **sem** worker de produção |
| **`tests/API.IntegrationTests/Sync/OfflineToCloudPocTests.cs`** | PoC **3.6**: stack real do client (`SyncBackgroundWorker`, `SyncHttpClient`, `OfflineSyncPullApplier`) |

A “nuvem” nos testes automatizados é a API levantada via `WebApplicationFactory<Program>` com SQLite no `CoreDbContext` (mesmo pipeline EF/CQRS que SQL Server em produção).

## Pré-requisitos

- .NET 10 SDK
- Repositório clonado em `/home/rony/sysvet` (ou equivalente)

Não é necessário subir API, Blazor ou MAUI manualmente para o cenário automatizado.

## Cenário automatizado

### Comando

```bash
dotnet test tests/API.IntegrationTests/API.IntegrationTests.csproj \
  --filter OfflineToCloudPocTests
```

O job [`/.github/workflows/ci.yml`](../../.github/workflows/ci.yml) executa `dotnet test SaaS_Veterinario.ci.slnf`, que inclui estes testes (sem job adicional).

### O que é validado

1. **Happy path** — cadastro de tutor no SQLite local gera `OutboxMessage`; um ciclo do worker envia `POST /api/v1/sync/push`, confirma tutor em `GET /api/v1/tutors`, outbox com `ProcessedAt`, métricas `pending=0`, `errors=0`.
2. **Segundo client (pull)** — banco local vazio; após push de outro client, um ciclo aplica pull e traz tutor **sem** enfileirar outbox (`SuppressOutbox`).
3. **Dead-letter** — tipo de comando desconhecido → `Error` preenchido, `Sync.UnknownCommandType`, `errors=1`.

Harness: [`SyncPocHarness.cs`](../../tests/API.IntegrationTests/Sync/SyncPocHarness.cs). Métricas: [`SyncPocMetrics`](../../tests/API.IntegrationTests/Sync/SyncPocMetrics.cs) (`[POC-METRICS]` no output xUnit).

### Métricas (referência local)

Valores típicos em ambiente de desenvolvimento (ordem de grandeza; variam com CPU/IO):

| Cenário | elapsed_ms | pending | errors | processed |
|---------|------------|---------|--------|-----------|
| Happy path (1 tutor) | ~200–800 | 0 | 0 | 1 |
| Pull segundo client | ~200–800 | 0 | 0 | 0 |
| Comando desconhecido | ~200–800 | 0 | 1 | 0 |

O worker também registra log estruturado ao fim de cada ciclo: `Sync cycle completed in {ElapsedMs}ms. Pending={PendingCount} Errors={ErrorCount}`.

## Apêndice — validação manual (Blazor / MAUI)

A UI **não** é automatizada nesta SP. Para inspeção visual:

1. Subir a API (`dotnet run --project src/API/API.csproj`) e o client desejado (`BlazorWeb` ou `MauiApp`).
2. Fazer login (usuário com policy `ClinicStaff`).
3. DevTools do browser (WASM): **Network → Offline** (ou desligar rede no SO no MAUI).
4. Cadastrar tutor e/ou pet nas telas CRM (SharedUI).
5. Confirmar badge **Offline** ([`SyncStatusBadge`](../../src/Clients/SharedUI/Components/SyncStatusBadge.razor)).
6. Restaurar conectividade; aguardar badge **Sincronizando...** e depois **Online** (worker ~30s ou wake on reconnect).
7. Conferir tutor/pet na API (Scalar em Development ou `GET /api/v1/tutors` / `pets`).

## Limitações conhecidas da PoC

- **Escopo sync client:** tutor/pet (CRM) + **appointments** (4.1) + **medical records** (4.2) + **templates/receitas/exames/metadados de anexo** (4.3) + **protocolos de vacina (pull) e doses (outbox push)** (4.4) + **orçamentos clínicos (pull + outbox push)** (4.5) + **recintos/leitos (pull) e internações clínicas (pull + outbox push)** (4.6); **bytes de anexo não** entram no pull — upload/download só online. Configuração de recintos na UI usa API online (`IWardUnitApiService`).
- **Worker WASM:** sincroniza enquanto a aba PWA permanece aberta.
- **Persistência WASM:** snapshot IndexedDB copia o arquivo `.db` inteiro ([ADR-014](./ADR-014-sqlite-local-clients.md)).
- **Conflitos CRM:** Last-Write-Wins por `UpdatedAt` / `OccurredAt`; merge por campo reservado ao domínio clínico.
- **Push:** FIFO por `CreatedAt`, lote até 50, **stop-on-first-error**; idempotência via `OutboxMessage.Id`.
- **Pull:** páginas de até 100; cursor em `SyncState.LastPullAt`; tombstones com soft delete.
- **API:** ingestão **síncrona** em `POST /api/v1/sync/push` (sem fila no servidor).
- **SQLite client:** filtro de `NextRetryAt` aplicado em memória após leitura (limitação do provider com `DateTimeOffset`).
- **Pull server:** `SyncChangeFeedReader` carrega candidatos e filtra `UpdatedAt` em memória (workaround SQLite no CI).
- **Pull client:** DTOs inválidos (e-mail/CPF/telefone) são ignorados silenciosamente no upsert.
- **Testes:** nuvem simulada com SQLite; produção usa SQL Server com o mesmo código de aplicação.
- **Observabilidade:** métricas PoC nos testes e logs do worker; sem OpenTelemetry dedicado nesta fase.

## Diagrama

Sequência completa: [`docs/diagramas/sync-sequence.mmd`](../diagramas/sync-sequence.mmd).

## Relacionados

- [roadmap.md](../roadmap.md) § 3.6
- [ADR-002](./ADR-002-estrategia-de-sync.md), [ADR-014](./ADR-014-sqlite-local-clients.md)
- [agents.md](../agents.md) — TDD, documentação viva
