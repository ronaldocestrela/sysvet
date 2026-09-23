using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Coupon redemption persistence port (9.5).</summary>
public interface ICouponRedemptionRepository
{
    /// <summary>True when tenant already redeemed the coupon.</summary>
    Task<bool> ExistsForTenantAsync(Guid tenantId, Guid couponId, CancellationToken cancellationToken = default);

    /// <summary>Gets pending redemption for tenant coupon awaiting invoice.</summary>
    Task<CouponRedemption?> GetPendingByTenantAndCouponAsync(
        Guid tenantId,
        Guid couponId,
        CancellationToken cancellationToken = default);

    /// <summary>Persists redemption.</summary>
    Task AddAsync(CouponRedemption redemption, CancellationToken cancellationToken = default);
}
