using Platform.Domain.Entities;

namespace Platform.Domain.Services;

/// <summary>Credit-card retry rules for overdue SaaS invoices (9.5).</summary>
public static class CardRetryPolicy
{
    /// <summary>Default maximum automatic retries after initial charge.</summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>Delay before the next retry attempt.</summary>
    public static readonly TimeSpan RetryInterval = TimeSpan.FromDays(1);

    /// <summary>Returns true when another gateway attempt is allowed.</summary>
    public static bool ShouldRetry(BillingInvoice invoice, DateTimeOffset asOfUtc, int maxRetries = DefaultMaxRetries)
    {
        if (invoice.Status is not BillingInvoiceStatus.Open and not BillingInvoiceStatus.Failed)
        {
            return false;
        }

        if (invoice.CardRetryCount >= maxRetries)
        {
            return false;
        }

        return invoice.NextCardRetryAt is null || asOfUtc >= invoice.NextCardRetryAt;
    }

    /// <summary>Records a retry attempt schedule on the invoice.</summary>
    public static void RecordAttemptScheduled(BillingInvoice invoice, DateTimeOffset asOfUtc)
    {
        invoice.IncrementCardRetry(asOfUtc.Add(RetryInterval));
    }
}
