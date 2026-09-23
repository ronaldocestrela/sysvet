using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Append-only clinic staff login attempt row for Super Admin review (9.7).</summary>
public sealed class PlatformLoginLog : Entity
{
    /// <summary>Tenant when the user exists; null for unknown email.</summary>
    public Guid? TenantId { get; private set; }

    /// <summary>Normalized email used at login.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Whether credentials validated.</summary>
    public bool Succeeded { get; private set; }

    /// <summary>Client IP at attempt time.</summary>
    public string ClientIp { get; private set; } = string.Empty;

    /// <summary>Raw User-Agent header.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    /// <summary>Geo country code or placeholder.</summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>Geo region or placeholder.</summary>
    public string Region { get; private set; } = string.Empty;

    /// <summary>When the attempt occurred (UTC).</summary>
    public DateTimeOffset OccurredAt { get; private set; }

#pragma warning disable CS8618
    private PlatformLoginLog()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a login audit row.</summary>
    public static Result<PlatformLoginLog> Create(
        Guid? tenantId,
        string email,
        bool succeeded,
        string clientIp,
        string userAgent,
        string country,
        string region,
        DateTimeOffset occurredAtUtc)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result.Failure<PlatformLoginLog>(ErrorCodes.Audit.InvalidEmail);
        }

        return Result.Success(new PlatformLoginLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId is { } id && id != Guid.Empty ? id : null,
            Email = normalizedEmail,
            Succeeded = succeeded,
            ClientIp = string.IsNullOrWhiteSpace(clientIp) ? "unknown" : clientIp.Trim(),
            UserAgent = Truncate(userAgent, 512),
            Country = string.IsNullOrWhiteSpace(country) ? "unknown" : country.Trim(),
            Region = string.IsNullOrWhiteSpace(region) ? "unknown" : region.Trim(),
            OccurredAt = occurredAtUtc,
            UpdatedAt = occurredAtUtc,
            RowVersion = new byte[8]
        });
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
