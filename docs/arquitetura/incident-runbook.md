# Runbook — Incidentes e status page (Fase 10.7)

Referência: [ADR-060](./ADR-060-carga-rollout-status.md).

## Canais

- **Operadores:** alertas OpenTelemetry / log `OperationalAlert` ([ADR-058](./ADR-058-backup-dr-observabilidade.md)): `Http5xx`, `SyncPushFailure`, `BillingChargeFailure`.
- **Clientes:** JSON público `GET /api/v1/public/status` (status page externa consome este endpoint).
- **Publicação:** Super Admin — PlatformWeb `/status-incidents` ou API `/api/v1/platform/status/incidents`.

## Severidade → impacto

| Situação | `StatusIncidentImpact` | Componentes típicos |
|----------|------------------------|---------------------|
| Degradação leve | Minor | sync |
| Falha billing SaaS | Major | billing |
| API indisponível | Critical | api, database |

## Fluxo

1. Confirmar sintoma (`GET /health`, dashboards APM).
2. Abrir incidente (título claro, componentes afetados).
3. Comunicar link da status page quando aplicável.
4. Mitigar e validar `GET /api/v1/public/status` → `OverallStatus` recuperando.
5. Resolver incidente (`PATCH .../resolve`); verificar remoção da lista pública.

## Encerramento pós-mortem (opcional)

Registrar causa raiz e follow-up fora deste repositório; manter trilha de auditoria Super Admin (`ReleaseRingSet`, change audits).
