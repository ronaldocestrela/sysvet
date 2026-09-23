using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Platform promotional coupon for SaaS billing (9.5).</summary>
public sealed class Coupon : Entity
{
    /// <summary>Normalized redeem code.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Percent or fixed discount.</summary>
    public CouponDiscountType DiscountType { get; private set; }

    /// <summary>Percent (0–100) or fixed BRL amount.</summary>
    public decimal Value { get; private set; }

    /// <summary>Maximum redemptions across all tenants; null = unlimited.</summary>
    public int? MaxRedemptions { get; private set; }

    /// <summary>Redemptions consumed when invoices open.</summary>
    public int RedemptionCount { get; private set; }

    /// <summary>UTC expiry; null = no expiry.</summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>When false, new redemptions are rejected.</summary>
    public bool IsActive { get; private set; } = true;

#pragma warning disable CS8618
    private Coupon()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates an active coupon.</summary>
    public static Result<Coupon> Create(
        string code,
        CouponDiscountType discountType,
        decimal value,
        int? maxRedemptions,
        DateTimeOffset? expiresAt)
    {
        var normalized = NormalizeCode(code);
        if (string.IsNullOrEmpty(normalized))
        {
            return Result.Failure<Coupon>(ErrorCodes.Coupon.InvalidCode);
        }

        if (discountType == CouponDiscountType.Percent && (value <= 0 || value > 100))
        {
            return Result.Failure<Coupon>(ErrorCodes.Coupon.InvalidValue);
        }

        if (discountType == CouponDiscountType.FixedAmount && value <= 0)
        {
            return Result.Failure<Coupon>(ErrorCodes.Coupon.InvalidValue);
        }

        if (maxRedemptions is <= 0)
        {
            return Result.Failure<Coupon>(ErrorCodes.Coupon.InvalidMaxRedemptions);
        }

        return Result.Success(new Coupon
        {
            Id = Guid.NewGuid(),
            Code = normalized,
            DiscountType = discountType,
            Value = value,
            MaxRedemptions = maxRedemptions,
            ExpiresAt = expiresAt,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Normalizes coupon codes for lookup.</summary>
    public static string NormalizeCode(string code) =>
        System.String.IsNullOrWhiteSpace(code) ? System.String.Empty : code.Trim().ToUpperInvariant();

    /// <summary>Whether the coupon can be redeemed at the given instant.</summary>
    public bool CanRedeem(DateTimeOffset asOfUtc, bool tenantAlreadyRedeemed)
    {
        if (!IsActive || tenantAlreadyRedeemed)
        {
            return false;
        }

        if (ExpiresAt is not null && asOfUtc > ExpiresAt)
        {
            return false;
        }

        if (MaxRedemptions is not null && RedemptionCount >= MaxRedemptions)
        {
            return false;
        }

        return true;
    }

    /// <summary>Computes discount amount capped at subtotal.</summary>
    public decimal CalculateDiscount(decimal subtotal)
    {
        if (subtotal <= 0)
        {
            return 0m;
        }

        var discount = DiscountType switch
        {
            CouponDiscountType.Percent => Math.Round(subtotal * (Value / 100m), 2, MidpointRounding.AwayFromZero),
            CouponDiscountType.FixedAmount => Value,
            _ => 0m
        };

        return Math.Min(subtotal, discount);
    }

    /// <summary>Increments redemption count when an invoice consumes the coupon.</summary>
    public Result RecordRedemptionConsumed()
    {
        if (MaxRedemptions is not null && RedemptionCount >= MaxRedemptions)
        {
            return Result.Failure(ErrorCodes.Coupon.Exhausted);
        }

        RedemptionCount++;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Deactivates the coupon.</summary>
    public Result Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
