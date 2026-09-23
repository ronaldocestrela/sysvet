using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class BillingWebhookReceiptRepository : IBillingWebhookReceiptRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public BillingWebhookReceiptRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default) =>
        _context.BillingWebhookReceipts.AnyAsync(r => r.IdempotencyKey == idempotencyKey, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(BillingWebhookReceipt receipt, CancellationToken cancellationToken = default) =>
        await _context.BillingWebhookReceipts.AddAsync(receipt, cancellationToken);
}
