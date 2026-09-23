using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Proration adjustment persistence port.</summary>
public interface ISubscriptionAdjustmentRepository
{
    /// <summary>Persists adjustment row.</summary>
    Task AddAsync(SubscriptionAdjustment adjustment, CancellationToken cancellationToken = default);

    /// <summary>Lists pending billing adjustments for tenant.</summary>
    Task<IReadOnlyList<SubscriptionAdjustment>> ListPendingByTenantIdAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>Lists adjustments linked to invoice.</summary>
    Task<IReadOnlyList<SubscriptionAdjustment>> ListByInvoiceIdAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default);
}
