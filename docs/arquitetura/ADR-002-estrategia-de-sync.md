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

### Regras operacionais (pós-PoC)

- **Conflitos:** Last-Write-Wins por `UpdatedAt` (UTC); agregados críticos com `RowVersion` / resposta `409 Conflict`.
- **Merge granular:** opcional em fichas clínicas quando campos offline não colidem.
- **Pull:** endpoint paginado por timestamp/cursor (ex. `GET /api/v1/sync/pull?since=…`); cliente faz upsert local.
- **Ordem:** FIFO por `CreatedAt`; **stop-on-first-error** no lote (dependências FK — ex. Tutor antes de Pet).
- **Falhas:** backoff exponencial em erros transitórios; dead-letter / marcação de erro permanente para validação `400`.

Implementação completa do motor na API (ingestão idempotente, pull, dead-letter) permanece no escopo da Fase 3.5 do [roadmap](../roadmap.md).

## Consequências

- **Positivas:** alinhamento com TDD e repositório offline único; sem tabelas de tracking de terceiros no SQL Server.
- **Negativas:** mais código de infra nos clientes e na API de sync.
- **Pendências:** endpoints de sync definitivos e sync bidirecional tutor/pet conforme roadmap 3.5.

## Confirmação no código

- [`src/Clients/Clients.Infrastructure/OfflineDbContext.cs`](../../src/Clients/Clients.Infrastructure/OfflineDbContext.cs) — persistência local.
- [`OutboxMessage`](../../src/Clients/Clients.Infrastructure/Sync/OutboxMessage.cs) — fila outbox.
- [`SyncBackgroundWorker`](../../src/Clients/Clients.Infrastructure/Sync/SyncBackgroundWorker.cs) — poll ~30s, lote até 50 mensagens, ordenação FIFO, `ISyncHttpClient.PushAsync`.
- Referências: [`BlazorWeb.csproj`](../../src/Clients/BlazorWeb/BlazorWeb.csproj), [`MauiApp.csproj`](../../src/Clients/MauiApp/MauiApp.csproj) → `Clients.Infrastructure`.
- PoC: [`tests/PoC.SyncTests/`](../../tests/PoC.SyncTests/).

## Relacionados

- [ADR-001](./ADR-001-monolito-modular.md), [ADR-003](./ADR-003-multi-tenancy.md) (`TenantId` no sync)
- [sync-sequence.mmd](../diagramas/sync-sequence.mmd)
- Cópia legada na raiz: [`ADR_002_Sincronizacao.md`](../../ADR_002_Sincronizacao.md) (aponta para este arquivo)
