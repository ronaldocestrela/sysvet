# Runbook — Rollout canary → beta → GA (Fase 10.7)

Referência: [ADR-060](./ADR-060-carga-rollout-status.md).

## Artefato

- API: artefato CI `sysvet-api-release` (GitHub Actions).
- Clients: Blazor PWA / PlatformWeb conforme pipeline existente.

## Sequência

| Etapa | Ação | Critério de avanço |
|-------|------|-------------------|
| 1 Canary | Marcar **1 tenant interno** como `ReleaseRing=Canary` via PlatformWeb ou `PATCH /api/v1/platform/tenants/{id}/release-ring` | Smoke: rotas 10.4 + `GET /health/ready` OK |
| 2 Beta | Promover cohorte piloto para `Beta` | Erro HTTP &lt; 1% em staging; sem alerta `ops-*` crítico |
| 3 GA | Demais tenants em `GeneralAvailability` | Drill registrado abaixo |

## Rollback

1. Republicar artefato API da versão anterior.
2. Restore de banco **somente** via [backup-dr-runbook.md](./backup-dr-runbook.md) (RTO 4 h).

## Evidência de drill

| Data | Versão / artefato | Anel | Resultado | Operador |
|------|-------------------|------|-----------|----------|
| | | | | |

Aceite roadmap 10.7: **rollout executado** = linha preenchida nesta tabela após drill em staging ou produção.
