using Core.Domain;
using Core.Domain.Auditing;

namespace Veterinary.Application.Quotes;

/// <summary>Sanitized audit entries for clinical quotes (no line PHI).</summary>
public static class ClinicalQuoteAuditHelper
{
    /// <summary>Logs a quote lifecycle action without monetary detail in payload.</summary>
    public static Task LogAsync(
        IAuditLogger auditLogger,
        ITenantContext tenantContext,
        Guid quoteId,
        string action,
        string fieldSummary,
        CancellationToken cancellationToken) =>
        auditLogger.LogAsync(
            tenantContext.TenantId,
            tenantContext.UserId,
            quoteId,
            "ClinicalQuote",
            action,
            $"ClinicalQuote {quoteId:N}: {fieldSummary}",
            cancellationToken);
}
