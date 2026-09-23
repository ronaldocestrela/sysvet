using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class BillingPaymentMethodRepository : IBillingPaymentMethodRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public BillingPaymentMethodRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<BillingPaymentMethod?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.BillingPaymentMethods.FirstOrDefaultAsync(m => m.TenantId == tenantId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(BillingPaymentMethod method, CancellationToken cancellationToken = default) =>
        await _context.BillingPaymentMethods.AddAsync(method, cancellationToken);
}
