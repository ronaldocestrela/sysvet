using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Tenant redemption of a platform coupon (9.5).</summary>
public sealed class CouponRedemption : Entity
{
    /// <summary>Coupon redeemed.</summary>
    public Guid CouponId { get; private set; }

    /// <summary>Tenant that redeemed.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Invoice that applied the discount; null until first open invoice.</summary>
    public Guid? BillingInvoiceId { get; private set; }

    /// <summary>When redemption was recorded.</summary>
    public DateTimeOffset RedeemedAt { get; private set; }

    /// <summary>Navigation.</summary>
    public Coupon? Coupon { get; private set; }

#pragma warning disable CS8618
    private CouponRedemption()
    {
    }
#pragma warning restore CS8618

    /// <summary>Records a pending redemption before billing.</summary>
    public static Result<CouponRedemption> Create(Guid couponId, Guid tenantId, DateTimeOffset redeemedAtUtc)
    {
        if (couponId == Guid.Empty || tenantId == Guid.Empty)
        {
            return Result.Failure<CouponRedemption>(ErrorCodes.Coupon.InvalidReference);
        }

        return Result.Success(new CouponRedemption
        {
            Id = Guid.NewGuid(),
            CouponId = couponId,
            TenantId = tenantId,
            RedeemedAt = redeemedAtUtc,
            UpdatedAt = redeemedAtUtc,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Links redemption to the invoice that consumed the discount.</summary>
    public Result AttachInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidTenant);
        }

        if (BillingInvoiceId is not null)
        {
            return Result.Success();
        }

        BillingInvoiceId = invoiceId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
