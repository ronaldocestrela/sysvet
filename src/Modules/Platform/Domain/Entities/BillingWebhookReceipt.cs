using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Idempotent webhook processing ledger (dbo).</summary>
public sealed class BillingWebhookReceipt : Entity
{
    /// <summary>Unique idempotency key (event + payment id).</summary>
    public string IdempotencyKey { get; private set; } = string.Empty;

    /// <summary>When the webhook was applied (UTC).</summary>
    public DateTimeOffset ProcessedAt { get; private set; }

#pragma warning disable CS8618
    private BillingWebhookReceipt()
    {
    }
#pragma warning restore CS8618

    /// <summary>Records successful webhook handling.</summary>
    public static BillingWebhookReceipt Create(string idempotencyKey, DateTimeOffset processedAtUtc)
    {
        return new BillingWebhookReceipt
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            ProcessedAt = processedAtUtc,
            UpdatedAt = processedAtUtc,
            RowVersion = new byte[8]
        };
    }
}
