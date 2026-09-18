using Core.Domain;
using Core.Domain.Auditing;

namespace Veterinary.Application.MedicalRecords;

/// <summary>Writes sanitized audit entries for medical record mutations (no clinical text).</summary>
public static class MedicalRecordAuditHelper
{
    /// <summary>Logs a medical record action without PHI in the payload.</summary>
    public static Task LogAsync(
        IAuditLogger auditLogger,
        ITenantContext tenantContext,
        Guid medicalRecordId,
        string action,
        string fieldSummary,
        CancellationToken cancellationToken) =>
        auditLogger.LogAsync(
            tenantContext.TenantId,
            tenantContext.UserId,
            medicalRecordId,
            "MedicalRecord",
            action,
            $"MedicalRecord {medicalRecordId:N}: {fieldSummary}",
            cancellationToken);
}
