namespace Platform.Domain.Entities;

/// <summary>How a platform coupon reduces invoice amount (9.5).</summary>
public enum CouponDiscountType
{
    /// <summary>Percentage off subtotal (0–100).</summary>
    Percent = 0,

    /// <summary>Fixed BRL amount off subtotal.</summary>
    FixedAmount = 1
}
