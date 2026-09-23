# Backup and disaster recovery runbook (Fase 10.5)

Operational runbook for SysVet / VetNexus SQL Server backups and quarterly restore validation.

## Goals

| Metric | Target |
|--------|--------|
| RPO | 24 hours (daily full backup) |
| RTO | 4 hours (documented restore steps) |
| Retention | 14 days (configurable via `Backup:RetentionDays`) |
| Drill frequency | Quarterly |

## Prerequisites

- SQL Server hosting the SysVet database (single database, schema-per-tenant per ADR-003).
- Operator account with `BACKUP DATABASE` and `RESTORE DATABASE` — **not** the application connection string user.
- `sqlcmd` on the operator host.
- Backup directory with sufficient disk (include growth from clinical blobs metadata and multi-tenant schemas).

## Daily backup (production)

Schedule via cron or SQL Agent (example cron at 02:00 UTC):

```bash
export SQL_PASSWORD='***'
export DATABASE_NAME=SysVet
export BACKUP_DIR=/var/backups/sysvet
/usr/bin/sqlserver-backup-only.sh   # or inline BACKUP only from drill script without restore section
```

For backup-only, run the first segments of [`scripts/sqlserver-backup-restore-drill.sh`](../../scripts/sqlserver-backup-restore-drill.sh) up to `RESTORE VERIFYONLY`, or use the T-SQL from [`SqlServerBackupPlan`](../../src/Modules/Core/Application/Operations/SqlServerBackupPlan.cs) without restore steps.

Backup flags (mandatory):

- `WITH COMPRESSION, CHECKSUM, STATS`

## Quarterly DR drill

1. Announce maintenance window (drill uses a scratch database `{DatabaseName}_drill`).
2. Run the full script:

```bash
chmod +x scripts/sqlserver-backup-restore-drill.sh
export SQL_SERVER=your-server
export SQL_USER=backup_operator
export SQL_PASSWORD='***'
export DATABASE_NAME=SysVet
export BACKUP_DIR=/var/backups/sysvet
./scripts/sqlserver-backup-restore-drill.sh
```

3. Record evidence:

| Field | Value |
|-------|--------|
| Date (UTC) | |
| Operator | |
| Backup file path | |
| `RESTORE VERIFYONLY` | Pass / Fail |
| Sentinel `PlatformTenants` before | |
| Sentinel `PlatformTenants` after restore | |
| Scratch DB dropped | Yes |

4. Store evidence in your ticket/wiki; attach script stdout.

## Sentinel

The drill compares `COUNT(*)` from `[SysVet].dbo.PlatformTenants` before backup and after restore into `{SysVet}_drill`. Mismatch fails the script.

## What restore does **not** cover

- **Client offline outbox** still pending on devices until sync completes (ADR-002).
- **Clinical blob files** if stored outside SQL (ADR-015) — restore blob storage separately.
- **Per-tenant schema restore** — not supported; only full database restore.

## Application observability (related)

- Ops health checks (`ops-*`) on `GET /health` only — see [`configuracao.md`](./configuracao.md).
- Alert logs: scope `OperationalAlert` with names `Http5xx`, `SyncPushFailure`, `BillingChargeFailure`.

## Rollback after failed drill

If the scratch database remains:

```sql
ALTER DATABASE [SysVet_drill] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE [SysVet_drill];
```

Production database is untouched except for the backup file created during the drill.
