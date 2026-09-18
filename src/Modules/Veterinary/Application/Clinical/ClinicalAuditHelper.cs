using Core.Domain;
using Core.Domain.Auditing;

namespace Veterinary.Application.Clinical;

/// <summary>Sanitized audit entries for clinical artifacts (no PHI).</summary>
public static class ClinicalAuditHelper
{
    /// <summary>Logs an action on a clinical entity without sensitive payload.</summary>
    public static Task LogAsync(
        IAuditLogger auditLogger,
        ITenantContext tenantContext,
        Guid entityId,
        string entityName,
        string action,
        string fieldSummary,
        CancellationToken cancellationToken) =>
        auditLogger.LogAsync(
            tenantContext.TenantId,
            tenantContext.UserId,
            entityId,
            entityName,
            action,
            $"{entityName} {entityId:N}: {fieldSummary}",
            cancellationToken);
}
