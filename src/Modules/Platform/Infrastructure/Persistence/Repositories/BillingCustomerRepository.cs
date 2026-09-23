using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class BillingCustomerRepository : IBillingCustomerRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public BillingCustomerRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<BillingCustomer?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.BillingCustomers.FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(BillingCustomer customer, CancellationToken cancellationToken = default) =>
        await _context.BillingCustomers.AddAsync(customer, cancellationToken);
}
