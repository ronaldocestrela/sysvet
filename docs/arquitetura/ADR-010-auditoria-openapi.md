# ADR 010: Auditoria append-only e contratos OpenAPI v1

## Status
Accepted

## Data
2026-09-16

## Contexto

O roadmap 2.6 exige trilha de auditoria consultável (quem, quando, entidade, ação, payload resumido), contrato HTTP único a partir de `Result.Failure` (ADR-005) e documentação OpenAPI/Scalar agrupável por módulo com versionamento `/api/v1/`.

## Decisão

1. **Escrita de auditoria (Core):** captura automática no `CoreDbContext.SaveChanges` apenas para entidades `IAuditable` (`Tutor`, `Pet`, `AccessProfile`), com ação semântica (`Added` / `Modified` / `Deleted`, incluindo soft delete), `EntityId`, payload JSON sanitizado e `UserId`/`TenantId` do `ITenantContext`. `IAuditLogger` permanece para integrações em outros DbContexts (ex.: Inventory).
2. **Consulta:** `GET /api/v1/audit-logs` paginado, policy `Admin` + permissão `Audit.Read`, menu `audit`.
3. **Erros:** `ResultExtensions.ToProblemDetails` + `ApiResultHelpers.RouteIdMismatch`; extensão `correlationId` via `AddProblemDetails` no host.
4. **OpenAPI:** documento `v1` com `info.version = 1.0.0`; tags duplas por rota (`Core`, `Tutors`, …) para filtro no Scalar; transformer `OpenApiModuleDocumentTransformer`.

## Consequências

- Admin consulta alterações de CRM e matriz de perfis no tenant.
- SQLite (Development) ordena auditoria em memória após filtros (limitação conhecida do provider).
- Leitura de auditoria carrega até o total filtrado antes da paginação em memória — aceitável no marco 2.6; otimizar se o volume crescer.

## Confirmação no código

- Domain: `AuditLog`, `IAuditable`, `IAuditLogRepository`, `Permissions.AuditRead`.
- Infrastructure: `AuditCaptureHelper`, `AuditLogRepository`, migration `AddAuditLogEntityId`.
- Application: `ListAuditLogsQuery` / handler.
- API: `AuditLogEndpointsExtensions`, `OpenApiModuleDocumentTransformer`, tags de módulo nos `*EndpointsExtensions`.

## Relacionados

- [ADR-005](./ADR-005-result-http.md)
- [ADR-008](./ADR-008-soft-delete-crm.md)
- [ADR-009](./ADR-009-access-profiles.md)
