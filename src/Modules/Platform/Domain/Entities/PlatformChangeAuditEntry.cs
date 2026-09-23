using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Append-only Super Admin configuration change audit (9.7).</summary>
public sealed class PlatformChangeAuditEntry : Entity
{
    /// <summary>Super Admin operator id.</summary>
    public string ActorUserId { get; private set; } = string.Empty;

    /// <summary>Affected tenant when applicable.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Semantic action name.</summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>Sanitized JSON summary.</summary>
    public string PayloadSummary { get; private set; } = string.Empty;

    /// <summary>Client IP of the operator.</summary>
    public string ClientIp { get; private set; } = string.Empty;

    /// <summary>When the change occurred (UTC).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

#pragma warning disable CS8618
    private PlatformChangeAuditEntry()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a change audit row.</summary>
    public static Result<PlatformChangeAuditEntry> Create(
        string actorUserId,
        Guid? tenantId,
        string action,
        string payloadSummary,
        string clientIp,
        DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(actorUserId))
        {
            return Result.Failure<PlatformChangeAuditEntry>(ErrorCodes.Audit.InvalidActor);
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            return Result.Failure<PlatformChangeAuditEntry>(ErrorCodes.Audit.InvalidAction);
        }

        return Result.Success(new PlatformChangeAuditEntry
        {
            Id = Guid.NewGuid(),
            ActorUserId = actorUserId.Trim(),
            TenantId = tenantId is { } id && id != Guid.Empty ? id : null,
            Action = action.Trim(),
            PayloadSummary = Truncate(payloadSummary, 2048),
            ClientIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim(),
            OccurredAt = occurredAtUtc,
            UpdatedAt = occurredAtUtc,
            RowVersion = new byte[8]
        });
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "{}";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
