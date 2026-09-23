using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Webhook idempotency ledger port (9.4).</summary>
public interface IBillingWebhookReceiptRepository
{
    /// <summary>Returns true when key was already processed.</summary>
    Task<bool> ExistsAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>Stores receipt.</summary>
    Task AddAsync(BillingWebhookReceipt receipt, CancellationToken cancellationToken = default);
}
