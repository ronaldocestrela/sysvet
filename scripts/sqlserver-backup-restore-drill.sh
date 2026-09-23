#!/usr/bin/env bash
set -euo pipefail

# SysVet SQL Server backup + quarterly DR drill (Fase 10.5).
# Requires sqlcmd and an operator login with BACKUP/RESTORE rights (not the app connection).

DATABASE_NAME="${DATABASE_NAME:-SysVet}"
BACKUP_DIR="${BACKUP_DIR:-/var/backups/sysvet}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"
SQLCMD="${SQLCMD:-sqlcmd}"
SERVER="${SQL_SERVER:-localhost}"
USER="${SQL_USER:-sa}"
PASSWORD="${SQL_PASSWORD:?Set SQL_PASSWORD}"

mkdir -p "${BACKUP_DIR}"

TIMESTAMP="$(date -u +%Y%m%d_%H%M%S)"
BACKUP_FILE="${BACKUP_DIR}/${DATABASE_NAME}_${TIMESTAMP}.bak"
DRILL_DB="${DATABASE_NAME}_drill"

run_sql() {
  "${SQLCMD}" -S "${SERVER}" -U "${USER}" -P "${PASSWORD}" -b -Q "$1"
}

echo "Recording sentinel before backup..."
SENTINEL_BEFORE="$("${SQLCMD}" -S "${SERVER}" -U "${USER}" -P "${PASSWORD}" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM [${DATABASE_NAME}].dbo.PlatformTenants;")"
SENTINEL_BEFORE="$(echo "${SENTINEL_BEFORE}" | tr -d '[:space:]')"

echo "Backing up ${DATABASE_NAME} to ${BACKUP_FILE}..."
run_sql "BACKUP DATABASE [${DATABASE_NAME}] TO DISK = N'${BACKUP_FILE}' WITH COMPRESSION, CHECKSUM, STATS = 5;"

echo "Verifying backup..."
run_sql "RESTORE VERIFYONLY FROM DISK = N'${BACKUP_FILE}' WITH CHECKSUM;"

if "${SQLCMD}" -S "${SERVER}" -U "${USER}" -P "${PASSWORD}" -h -1 -W -Q "SET NOCOUNT ON; SELECT DB_ID(N'${DRILL_DB}');" | grep -qv "^NULL$"; then
  run_sql "ALTER DATABASE [${DRILL_DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${DRILL_DB}];"
fi

DATA_FILE="${BACKUP_DIR}/${DRILL_DB}.mdf"
LOG_FILE="${BACKUP_DIR}/${DRILL_DB}_log.ldf"

echo "Restoring drill database ${DRILL_DB}..."
run_sql "RESTORE DATABASE [${DRILL_DB}] FROM DISK = N'${BACKUP_FILE}' WITH MOVE N'${DATABASE_NAME}' TO N'${DATA_FILE}', MOVE N'${DATABASE_NAME}_log' TO N'${LOG_FILE}', RECOVERY, CHECKSUM;"

SENTINEL_AFTER="$("${SQLCMD}" -S "${SERVER}" -U "${USER}" -P "${PASSWORD}" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM [${DRILL_DB}].dbo.PlatformTenants;")"
SENTINEL_AFTER="$(echo "${SENTINEL_AFTER}" | tr -d '[:space:]')"

if [[ "${SENTINEL_BEFORE}" != "${SENTINEL_AFTER}" ]]; then
  echo "Sentinel mismatch: before=${SENTINEL_BEFORE} after=${SENTINEL_AFTER}" >&2
  exit 1
fi

echo "Dropping drill database..."
run_sql "ALTER DATABASE [${DRILL_DB}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${DRILL_DB}];"

echo "Pruning backups older than ${RETENTION_DAYS} days..."
find "${BACKUP_DIR}" -name "${DATABASE_NAME}_*.bak" -type f -mtime +"${RETENTION_DAYS}" -delete

echo "DR drill completed successfully. Sentinel=${SENTINEL_BEFORE} Backup=${BACKUP_FILE}"
