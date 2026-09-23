using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Billing customer persistence port (9.4).</summary>
public interface IBillingCustomerRepository
{
    /// <summary>Gets customer by tenant.</summary>
    Task<BillingCustomer?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists new customer.</summary>
    Task AddAsync(BillingCustomer customer, CancellationToken cancellationToken = default);
}
