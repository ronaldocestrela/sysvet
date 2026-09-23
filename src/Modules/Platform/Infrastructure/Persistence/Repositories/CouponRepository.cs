using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class CouponRepository : ICouponRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public CouponRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<Coupon?> GetByCodeAsync(string normalizedCode, CancellationToken cancellationToken = default) =>
        _context.Coupons.FirstOrDefaultAsync(c => c.Code == normalizedCode, cancellationToken);

    /// <inheritdoc />
    public Task<Coupon?> GetByIdAsync(Guid couponId, CancellationToken cancellationToken = default) =>
        _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<Coupon>> ListAsync(CancellationToken cancellationToken = default) =>
        _context.Coupons
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Coupon>)t.Result, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Coupon coupon, CancellationToken cancellationToken = default) =>
        await _context.Coupons.AddAsync(coupon, cancellationToken);
}
