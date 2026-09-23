# ADR-052: Auditoria de login, API keys e health por tenant (9.7)

## Status
Accepted

## Data
2026-09-23

## Contexto

O backoffice §4 exige logs de login (IP, device, geo), auditoria de alterações Super Admin (planos, descontos), API keys para parceiros e métricas operacionais por tenant. A UI Super Admin permanece 9.8. O `AuditLog` do Core (ADR-010) é consultável pelo Admin da clínica; trilhas de plataforma ficam no catálogo `Platform` (dbo), como impersonation (ADR-051).

## Opções consideradas

1. **Reutilizar `AuditLog` do Core para mudanças de plano** — visível ao Admin do tenant; rejeitado.
2. **`PlatformChangeAuditEntry` append-only em dbo** — escolhido, alinhado a `ImpersonationAuditEntry`.
3. **Cache de API key em memória** — revogação não imediata; rejeitado.
4. **Lookup de hash no banco a cada request partner** — escolhido (aceite 9.7).
5. **Geo IP externo no MVP** — adiado; `IGeoIpLookup` default retorna `unknown`.

## Decisão

1. **`PlatformLoginLog`**: gravado após `POST /api/v1/auth/login` via `RecordPlatformLoginCommand`; tenant resolvido por e-mail quando ausente.
2. **`PlatformChangeAuditEntry`**: plano, add-on, feature flag, cupom e API key via `PlatformBackofficeAuditRecorder` na mesma UoW dos handlers 9.3–9.5.
3. **`PartnerApiKey`**: segredo `vn_*`, hash SHA-256 único; revogação preenche `RevokedAt`.
4. **Partner `GET /api/v1/partner/health`**: header `X-Api-Key`; `PartnerApiKeyMiddleware` valida hash no banco sem cache.
5. **`TenantRequestDaily`**: middleware pós-pipeline incrementa contador por `TenantId` resolvido; falha de métrica não quebra a API.
6. **Volume de dados**: `ITenantDataVolumeContributor`; Core conta tutores+pets com `IgnoreQueryFilters`; `EstimatedBytes = rows × 512` (estimativa documentada, não tamanho em disco).

## Consequências

- Aceite: key revogada → 401 imediato; health por tenant isolado; login log com IP/User-Agent.
- SQLite: ordenação de logs/auditoria em memória após filtros (mesmo padrão ADR-010).
- Métricas SaaS (MRR/churn) permanecem Fase 10.

## Confirmação no código

- Domain: `PlatformLoginLog`, `PlatformChangeAuditEntry`, `PartnerApiKey`, `TenantRequestDaily`
- Application: auditing, API keys, `GetTenantHealthQuery`
- API: `/api/v1/platform/login-logs`, `/change-audits`, `/tenants/{id}/health`, `/api-keys`, `/api/v1/partner/health`
- Middleware: `PartnerApiKeyMiddleware`, `TenantRequestMetricsMiddleware`

## Relacionados

- [ADR-010](./ADR-010-auditoria-openapi.md), [ADR-051](./ADR-051-nfse-saas-impersonation.md)
- [roadmap](../roadmap.md) §9.7, [backoffice](../backoffice.md) §4
