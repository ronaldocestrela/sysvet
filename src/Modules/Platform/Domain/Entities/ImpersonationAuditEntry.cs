using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Append-only impersonation audit row (9.6).</summary>
public sealed class ImpersonationAuditEntry : Entity
{
    /// <summary>Related session id.</summary>
    public Guid SessionId { get; private set; }

    /// <summary>Support operator user id.</summary>
    public string ActorUserId { get; private set; } = string.Empty;

    /// <summary>Target tenant id.</summary>
    public Guid TargetTenantId { get; private set; }

    /// <summary>Event kind (Started / Ended).</summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>When the event occurred (UTC).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>Client IP for the event.</summary>
    public string ClientIp { get; private set; } = string.Empty;

#pragma warning disable CS8618
    private ImpersonationAuditEntry()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a started audit entry.</summary>
    public static Result<ImpersonationAuditEntry> CreateStarted(
        Guid sessionId,
        string actorUserId,
        Guid targetTenantId,
        string clientIp,
        DateTimeOffset occurredAtUtc) =>
        CreateInternal(sessionId, actorUserId, targetTenantId, "Started", clientIp, occurredAtUtc);

    /// <summary>Creates an ended audit entry.</summary>
    public static Result<ImpersonationAuditEntry> CreateEnded(
        Guid sessionId,
        string actorUserId,
        Guid targetTenantId,
        string clientIp,
        DateTimeOffset occurredAtUtc) =>
        CreateInternal(sessionId, actorUserId, targetTenantId, "Ended", clientIp, occurredAtUtc);

    private static Result<ImpersonationAuditEntry> CreateInternal(
        Guid sessionId,
        string actorUserId,
        Guid targetTenantId,
        string action,
        string clientIp,
        DateTimeOffset occurredAtUtc)
    {
        if (sessionId == Guid.Empty || targetTenantId == Guid.Empty || string.IsNullOrWhiteSpace(actorUserId))
        {
            return Result.Failure<ImpersonationAuditEntry>(ErrorCodes.Impersonation.InvalidActor);
        }

        return Result.Success(new ImpersonationAuditEntry
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            ActorUserId = actorUserId.Trim(),
            TargetTenantId = targetTenantId,
            Action = action,
            OccurredAt = occurredAtUtc,
            ClientIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim(),
            UpdatedAt = occurredAtUtc,
            RowVersion = new byte[8]
        });
    }
}
