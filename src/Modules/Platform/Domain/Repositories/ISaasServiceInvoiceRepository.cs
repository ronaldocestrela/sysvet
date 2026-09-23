using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Persistence port for VetNexus NFS-e rows (9.6).</summary>
public interface ISaasServiceInvoiceRepository
{
    /// <summary>Gets NFS-e by billing invoice id.</summary>
    Task<SaasServiceInvoice?> GetByBillingInvoiceIdAsync(Guid billingInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>Adds a new NFS-e row.</summary>
    Task AddAsync(SaasServiceInvoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Lists NFS-e rows for a tenant.</summary>
    Task<IReadOnlyList<SaasServiceInvoice>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
