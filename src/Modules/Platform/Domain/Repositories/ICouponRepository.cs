using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Platform coupon catalog port (9.5).</summary>
public interface ICouponRepository
{
    /// <summary>Gets coupon by normalized code.</summary>
    Task<Coupon?> GetByCodeAsync(string normalizedCode, CancellationToken cancellationToken = default);

    /// <summary>Gets coupon by id.</summary>
    Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken cancellationToken = default);

    /// <summary>Lists coupons newest first.</summary>
    Task<IReadOnlyList<Coupon>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists a new coupon.</summary>
    Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default);
}
