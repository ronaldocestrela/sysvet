using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Billing payment method persistence port (9.4).</summary>
public interface IBillingPaymentMethodRepository
{
    /// <summary>Gets payment method by tenant.</summary>
    Task<BillingPaymentMethod?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists new payment method.</summary>
    Task AddAsync(BillingPaymentMethod method, CancellationToken cancellationToken = default);
}
