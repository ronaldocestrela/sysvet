using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class CouponRedemptionRepository : ICouponRedemptionRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public CouponRedemptionRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<bool> ExistsForTenantAsync(Guid tenantId, Guid couponId, CancellationToken cancellationToken = default) =>
        _context.CouponRedemptions.AnyAsync(r => r.TenantId == tenantId && r.CouponId == couponId, cancellationToken);

    /// <inheritdoc />
    public Task<CouponRedemption?> GetPendingByTenantAndCouponAsync(
        Guid tenantId,
        Guid couponId,
        CancellationToken cancellationToken = default) =>
        _context.CouponRedemptions.FirstOrDefaultAsync(
            r => r.TenantId == tenantId && r.CouponId == couponId && r.BillingInvoiceId == null,
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(CouponRedemption redemption, CancellationToken cancellationToken = default) =>
        await _context.CouponRedemptions.AddAsync(redemption, cancellationToken);
}
