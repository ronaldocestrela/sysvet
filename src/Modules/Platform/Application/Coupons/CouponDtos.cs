using Platform.Domain.Entities;

namespace Platform.Application.Coupons;

/// <summary>Coupon list item for Super Admin API.</summary>
public sealed record CouponSummaryDto(
    Guid Id,
    string Code,
    CouponDiscountType DiscountType,
    decimal Value,
    int? MaxRedemptions,
    int RedemptionCount,
    DateTimeOffset? ExpiresAt,
    bool IsActive);
