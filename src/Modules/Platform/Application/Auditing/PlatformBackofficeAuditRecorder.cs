using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Application.Auditing;

/// <summary>Stages append-only Super Admin change audit rows on the platform unit of work (9.7).</summary>
public sealed class PlatformBackofficeAuditRecorder
{
    private readonly IPlatformChangeAuditRepository _repository;
    private readonly IPlatformAuditContext _auditContext;

    /// <summary>Creates the recorder.</summary>
    public PlatformBackofficeAuditRecorder(
        IPlatformChangeAuditRepository repository,
        IPlatformAuditContext auditContext)
    {
        _repository = repository;
        _auditContext = auditContext;
    }

    /// <summary>Appends a change audit entry to the current transaction.</summary>
    public async Task RecordAsync(
        Guid? tenantId,
        string action,
        string payloadSummary,
        CancellationToken cancellationToken = default)
    {
        var entry = PlatformChangeAuditEntry.Create(
            _auditContext.ActorUserId,
            tenantId,
            action,
            payloadSummary,
            _auditContext.ClientIp,
            DateTimeOffset.UtcNow);
        if (entry.IsFailure)
        {
            return;
        }

        await _repository.AddAsync(entry.Value, cancellationToken);
    }
}
