using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Daily API request counter per tenant for health dashboards (9.7).</summary>
public sealed class TenantRequestDaily : Entity
{
    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>UTC calendar date bucket.</summary>
    public DateOnly DateUtc { get; private set; }

    /// <summary>Number of counted requests.</summary>
    public int RequestCount { get; private set; }

#pragma warning disable CS8618
    private TenantRequestDaily()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a counter row.</summary>
    public static Result<TenantRequestDaily> Create(Guid tenantId, DateTimeOffset asOfUtc, int initialCount)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TenantRequestDaily>(ErrorCodes.Health.InvalidTenant);
        }

        if (initialCount < 0)
        {
            return Result.Failure<TenantRequestDaily>(ErrorCodes.Health.InvalidCounter);
        }

        var date = DateOnly.FromDateTime(asOfUtc.UtcDateTime);
        return Result.Success(new TenantRequestDaily
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DateUtc = date,
            RequestCount = initialCount,
            UpdatedAt = asOfUtc,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Increments the daily request count.</summary>
    public Result Increment(DateTimeOffset asOfUtc)
    {
        RequestCount++;
        UpdatedAt = asOfUtc;
        return Result.Success();
    }
}
