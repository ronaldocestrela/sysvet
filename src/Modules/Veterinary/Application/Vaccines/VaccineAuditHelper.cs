using Core.Domain;
using Core.Domain.Auditing;

namespace Veterinary.Application.Vaccines;

/// <summary>Writes sanitized audit entries for vaccine mutations.</summary>
public static class VaccineAuditHelper
{
    /// <summary>Logs a vaccine-related action without sensitive batch details.</summary>
    public static Task LogAsync(
        IAuditLogger auditLogger,
        ITenantContext tenantContext,
        Guid entityId,
        string entityType,
        string action,
        string fieldSummary,
        CancellationToken cancellationToken) =>
        auditLogger.LogAsync(
            tenantContext.TenantId,
            tenantContext.UserId,
            entityId,
            entityType,
            action,
            $"{entityType} {entityId:N}: {fieldSummary}",
            cancellationToken);
}
