using Core.Application.Messaging;
using Platform.Domain.Entities;

namespace Platform.Application.Coupons;

/// <summary>Creates a platform coupon (Super Admin).</summary>
public sealed record CreateCouponCommand(
    string Code,
    CouponDiscountType DiscountType,
    decimal Value,
    int? MaxRedemptions,
    DateTimeOffset? ExpiresAt) : ICommand<CouponSummaryDto>;

/// <summary>Lists platform coupons.</summary>
public sealed record ListCouponsQuery : IQuery<IReadOnlyList<CouponSummaryDto>>;

/// <summary>Redeems a coupon for a tenant subscription.</summary>
public sealed record RedeemTenantCouponCommand(Guid TenantId, string Code) : ICommand;
