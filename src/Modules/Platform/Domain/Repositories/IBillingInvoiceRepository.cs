using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Platform invoice persistence port (9.4).</summary>
public interface IBillingInvoiceRepository
{
    /// <summary>Gets invoice by id with charges.</summary>
    Task<BillingInvoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>Gets open invoice for tenant if any.</summary>
    Task<BillingInvoice?> GetOpenByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Lists invoices for tenant newest first.</summary>
    Task<IReadOnlyList<BillingInvoice>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Persists invoice.</summary>
    Task AddAsync(BillingInvoice invoice, CancellationToken cancellationToken = default);
}
