using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class BillingInvoiceRepository : IBillingInvoiceRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public BillingInvoiceRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<BillingInvoice?> GetByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default) =>
        _context.BillingInvoices
            .Include(i => i.Charges)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

    /// <inheritdoc />
    public Task<BillingInvoice?> GetOpenByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.BillingInvoices
            .Include(i => i.Charges)
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Status == BillingInvoiceStatus.Open, cancellationToken);

    /// <inheritdoc />
    public async Task<BillingInvoice?> GetOutstandingByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.BillingInvoices
            .Include(i => i.Charges)
            .Where(i => i.TenantId == tenantId
                        && (i.Status == BillingInvoiceStatus.Open || i.Status == BillingInvoiceStatus.Failed))
            .ToListAsync(cancellationToken);

        return invoices
            .OrderByDescending(i => i.PeriodEnd)
            .FirstOrDefault();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BillingInvoice>> ListByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.BillingInvoices
            .AsNoTracking()
            .Include(i => i.Charges)
            .Where(i => i.TenantId == tenantId)
            .OrderByDescending(i => i.PeriodEnd)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<BillingInvoice>)t.Result, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(BillingInvoice invoice, CancellationToken cancellationToken = default) =>
        await _context.BillingInvoices.AddAsync(invoice, cancellationToken);
}
