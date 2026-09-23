# ADR-058: Backup, DR e observabilidade avançada (Fase 10.5)

## Status
Accepted

## Data
2026-09-23

## Contexto

A Fase 10.5 exige backup automático SQL Server com restore testado, APM e alertas operacionais (5xx, falha de push de sync, falha de cobrança). O repositório já possui health checks (`/health/live`, `/health/ready`), correlation id e logging JSON ([`configuracao.md`](./configuracao.md)); não há OpenTelemetry nem runbook de DR.

Multi-tenancy usa schema por tenant no mesmo database ([ADR-003](./ADR-003-multi-tenancy.md)). A outbox offline vive no SQLite do cliente ([ADR-002](./ADR-002-estrategia-de-sync.md)); o servidor observa falhas de push via `PushSyncBatchCommandHandler`.

## Opções consideradas

1. **Application Insights SDK** — acoplamento Azure; rejeitado como SDK primário.
2. **OpenTelemetry + OTLP opcional** — vendor-neutral; aceito (Application Insights futuro via collector OTLP).
3. **Backup pela API com permissões elevadas** — viola least privilege; rejeitado.
4. **Job externo (`sqlcmd`/SQL Agent) + planner testável no código** — aceito.

## Decisão

### DR (RPO/RTO revisáveis no runbook)

| Meta | Valor |
|------|-------|
| RPO | 24 h (full diário `CHECKSUM` + compressão) |
| RTO | 4 h |
| Retenção | 14 dias |
| Drill | Trimestral com evidência (sentinela `PlatformTenants`) |

### Observabilidade

1. **`Observability` options** — `OtlpEndpoint`, `ConsoleExporter`, `TraceSampleRatio`, limiares de alerta.
2. **OpenTelemetry** na API: ASP.NET Core, HttpClient, runtime; resource `service.name=sysvet-api`; meter `SysVet.Operations`.
3. **Alertas in-process** — `OperationalAlertWindow` + log estruturado `OperationalAlert`; health checks tag **`ops`** apenas em `GET /health` (não em `ready`).
4. **Sinais:** `Http5xx` (middleware), `SyncPushFailure` (`ISyncPushObserver`), `BillingChargeFailure` (`IBillingChargeObserver` + contagem de faturas `Failed` em aberto).

### Backup

1. **`SqlServerBackupPlan`** em Core.Application — gera T-SQL; não abre conexão.
2. **`scripts/sqlserver-backup-restore-drill.sh`** — operação com identidade de backup.
3. Runbook [`backup-dr-runbook.md`](./backup-dr-runbook.md).

## Consequências

- CI permanece SQLite; drill SQL Server é script + runbook, não Testcontainers nesta fase.
- Restore validado trimestralmente fora do pipeline (aceite roadmap).
- Falha de billing/sync não derruba readiness do balanceador.

## Confirmação no código

- [`SqlServerBackupPlan`](../../src/Modules/Core/Application/Operations/SqlServerBackupPlan.cs)
- [`OperationalAlertWindow`](../../src/Modules/Core/Application/Operations/OperationalAlertWindow.cs)
- [`OpenTelemetryServiceCollectionExtensions`](../../src/API/Extensions/OpenTelemetryServiceCollectionExtensions.cs)
- [`OperationalAlertServiceCollectionExtensions`](../../src/API/Extensions/OperationalAlertServiceCollectionExtensions.cs)
- [`scripts/sqlserver-backup-restore-drill.sh`](../../scripts/sqlserver-backup-restore-drill.sh)

## Relacionados

- [ADR-003](./ADR-003-multi-tenancy.md), [ADR-049](./ADR-049-gateway-assinatura-asaas.md), [ADR-057](./ADR-057-cache-paginacao-baseline.md)
- [`docs/roadmap.md`](../roadmap.md) §10.5
