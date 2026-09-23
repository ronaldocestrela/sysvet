using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Active or historical Super Admin impersonation of a tenant (9.6).</summary>
public sealed class ImpersonationSession : Entity
{
    /// <summary>Support operator user id (Identity).</summary>
    public string ActorUserId { get; private set; } = string.Empty;

    /// <summary>Actor e-mail at start time.</summary>
    public string ActorEmail { get; private set; } = string.Empty;

    /// <summary>Target tenant id.</summary>
    public Guid TargetTenantId { get; private set; }

    /// <summary>Client IP at session start.</summary>
    public string ClientIp { get; private set; } = string.Empty;

    /// <summary>UTC start.</summary>
    public DateTimeOffset StartedAt { get; private set; }

    /// <summary>UTC hard expiry.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>UTC end when explicitly closed or expired enforcement.</summary>
    public DateTimeOffset? EndedAt { get; private set; }

#pragma warning disable CS8618
    private ImpersonationSession()
    {
    }
#pragma warning restore CS8618

    /// <summary>Starts a new impersonation session.</summary>
    public static Result<ImpersonationSession> Start(
        Guid id,
        string actorUserId,
        string actorEmail,
        Guid targetTenantId,
        string clientIp,
        DateTimeOffset startedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (id == Guid.Empty || targetTenantId == Guid.Empty)
        {
            return Result.Failure<ImpersonationSession>(ErrorCodes.Impersonation.InvalidTarget);
        }

        if (string.IsNullOrWhiteSpace(actorUserId) || string.IsNullOrWhiteSpace(actorEmail))
        {
            return Result.Failure<ImpersonationSession>(ErrorCodes.Impersonation.InvalidActor);
        }

        if (expiresAtUtc <= startedAtUtc)
        {
            return Result.Failure<ImpersonationSession>(ErrorCodes.Impersonation.InvalidDuration);
        }

        return Result.Success(new ImpersonationSession
        {
            Id = id,
            ActorUserId = actorUserId.Trim(),
            ActorEmail = actorEmail.Trim(),
            TargetTenantId = targetTenantId,
            ClientIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim(),
            StartedAt = startedAtUtc,
            ExpiresAt = expiresAtUtc,
            UpdatedAt = startedAtUtc,
            RowVersion = new byte[8]
        });
    }

    /// <summary>True when the session accepts impersonation tokens.</summary>
    public bool IsActive(DateTimeOffset asOfUtc) =>
        EndedAt is null && asOfUtc < ExpiresAt;

    /// <summary>Ends the session once.</summary>
    public Result End(DateTimeOffset endedAtUtc)
    {
        if (EndedAt is not null)
        {
            return Result.Failure(ErrorCodes.Impersonation.AlreadyEnded);
        }

        EndedAt = endedAtUtc;
        UpdatedAt = endedAtUtc;
        return Result.Success();
    }
}
