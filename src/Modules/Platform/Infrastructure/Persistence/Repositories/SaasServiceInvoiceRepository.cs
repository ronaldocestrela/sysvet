using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class SaasServiceInvoiceRepository : ISaasServiceInvoiceRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public SaasServiceInvoiceRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<SaasServiceInvoice?> GetByBillingInvoiceIdAsync(Guid billingInvoiceId, CancellationToken cancellationToken = default) =>
        _context.SaasServiceInvoices.FirstOrDefaultAsync(i => i.BillingInvoiceId == billingInvoiceId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(SaasServiceInvoice invoice, CancellationToken cancellationToken = default) =>
        await _context.SaasServiceInvoices.AddAsync(invoice, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<SaasServiceInvoice>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.SaasServiceInvoices
            .AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<SaasServiceInvoice>)t.Result, cancellationToken);
}
