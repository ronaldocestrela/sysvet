using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Partner integration API key bound to a tenant (9.7).</summary>
public sealed class PartnerApiKey : Entity
{
    /// <summary>Tenant the key grants access to.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Human-readable partner label.</summary>
    public string PartnerName { get; private set; } = string.Empty;

    /// <summary>Visible prefix for support (not secret).</summary>
    public string KeyPrefix { get; private set; } = string.Empty;

    /// <summary>SHA-256 hex hash of the full secret.</summary>
    public string SecretHash { get; private set; } = string.Empty;

    /// <summary>Granted scope (e.g. health:read).</summary>
    public string Scope { get; private set; } = string.Empty;

    /// <summary>Super Admin user id that issued the key.</summary>
    public string CreatedByUserId { get; private set; } = string.Empty;

    /// <summary>When the key was created (UTC).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When revoked; null while active.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>True when the key can authenticate requests.</summary>
    public bool IsActive => RevokedAt is null;

#pragma warning disable CS8618
    private PartnerApiKey()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a new active API key metadata row.</summary>
    public static Result<PartnerApiKey> Create(
        Guid tenantId,
        string partnerName,
        string keyPrefix,
        string secretHash,
        string scope,
        string createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<PartnerApiKey>(ErrorCodes.ApiKey.InvalidTenant);
        }

        if (string.IsNullOrWhiteSpace(partnerName))
        {
            return Result.Failure<PartnerApiKey>(ErrorCodes.ApiKey.InvalidPartnerName);
        }

        if (string.IsNullOrWhiteSpace(keyPrefix) || string.IsNullOrWhiteSpace(secretHash))
        {
            return Result.Failure<PartnerApiKey>(ErrorCodes.ApiKey.InvalidSecret);
        }

        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            return Result.Failure<PartnerApiKey>(ErrorCodes.Audit.InvalidActor);
        }

        return Result.Success(new PartnerApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PartnerName = partnerName.Trim(),
            KeyPrefix = keyPrefix.Trim(),
            SecretHash = secretHash.Trim(),
            Scope = string.IsNullOrWhiteSpace(scope) ? PartnerApiKeyScopes.HealthRead : scope.Trim(),
            CreatedByUserId = createdByUserId.Trim(),
            CreatedAt = createdAtUtc,
            UpdatedAt = createdAtUtc,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Extracts a display prefix from a generated secret.</summary>
    public static string ExtractPrefix(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return string.Empty;
        }

        var trimmed = secret.Trim();
        return trimmed.Length <= 12 ? trimmed : trimmed[..12];
    }

    /// <summary>Revokes the key immediately.</summary>
    public Result Revoke(DateTimeOffset revokedAtUtc)
    {
        if (RevokedAt is not null)
        {
            return Result.Failure(ErrorCodes.ApiKey.AlreadyRevoked);
        }

        RevokedAt = revokedAtUtc;
        UpdatedAt = revokedAtUtc;
        return Result.Success();
    }
}
