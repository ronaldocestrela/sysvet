using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Platform SaaS invoice for one billing period (dbo).</summary>
public sealed class BillingInvoice : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Billing period start (UTC).</summary>
    public DateTimeOffset PeriodStart { get; private set; }

    /// <summary>Billing period end (UTC).</summary>
    public DateTimeOffset PeriodEnd { get; private set; }

    /// <summary>Total charged amount in BRL.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Invoice lifecycle status.</summary>
    public BillingInvoiceStatus Status { get; private set; }

    /// <summary>When payment was confirmed (UTC).</summary>
    public DateTimeOffset? PaidAt { get; private set; }

    /// <summary>Automatic card retry attempts after failure (9.5).</summary>
    public int CardRetryCount { get; private set; }

    /// <summary>Earliest UTC instant for the next card retry (9.5).</summary>
    public DateTimeOffset? NextCardRetryAt { get; private set; }

    /// <summary>Coupon discount applied when the invoice was opened (9.5).</summary>
    public Guid? AppliedCouponId { get; private set; }

    /// <summary>Gateway charges linked to this invoice.</summary>
    public ICollection<BillingCharge> Charges { get; private set; } = new List<BillingCharge>();

#pragma warning disable CS8618
    private BillingInvoice()
    {
    }
#pragma warning restore CS8618

    /// <summary>Opens a new invoice for the period.</summary>
    public static Result<BillingInvoice> Open(
        Guid tenantId,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        decimal amount)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<BillingInvoice>(ErrorCodes.Billing.InvalidTenant);
        }

        if (amount < 0)
        {
            return Result.Failure<BillingInvoice>(ErrorCodes.Billing.InvalidAmount);
        }

        return Result.Success(new BillingInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            Amount = amount,
            Status = amount == 0 ? BillingInvoiceStatus.Paid : BillingInvoiceStatus.Open,
            PaidAt = amount == 0 ? DateTimeOffset.UtcNow : null,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Marks invoice paid from webhook or local zero settlement.</summary>
    public Result MarkPaid(DateTimeOffset paidAtUtc)
    {
        if (Status is BillingInvoiceStatus.Paid)
        {
            return Result.Success();
        }

        if (Status is not (BillingInvoiceStatus.Open or BillingInvoiceStatus.Failed))
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        Status = BillingInvoiceStatus.Paid;
        PaidAt = paidAtUtc;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Marks invoice failed or overdue.</summary>
    public Result MarkFailed()
    {
        if (Status is BillingInvoiceStatus.Paid or BillingInvoiceStatus.Refunded)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        Status = BillingInvoiceStatus.Failed;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Cancels an open invoice.</summary>
    public Result Cancel()
    {
        if (Status is not BillingInvoiceStatus.Open)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        Status = BillingInvoiceStatus.Canceled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Records refund after prior payment.</summary>
    public Result MarkRefunded()
    {
        if (Status is not BillingInvoiceStatus.Paid)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        Status = BillingInvoiceStatus.Refunded;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Attaches gateway charge metadata.</summary>
    public BillingCharge AddCharge(string gatewayPaymentId, string? pixCopyPaste, string? boletoLine)
    {
        var charge = BillingCharge.Create(Id, gatewayPaymentId, pixCopyPaste, boletoLine);
        Charges.Add(charge);
        if (Status == BillingInvoiceStatus.Failed)
        {
            Status = BillingInvoiceStatus.Open;
        }

        UpdatedAt = DateTimeOffset.UtcNow;
        return charge;
    }

    /// <summary>Records coupon used when opening the invoice.</summary>
    public void SetAppliedCoupon(Guid couponId) => AppliedCouponId = couponId;

    /// <summary>Increments card retry counter and schedules the next attempt.</summary>
    public void IncrementCardRetry(DateTimeOffset nextRetryAtUtc)
    {
        CardRetryCount++;
        NextCardRetryAt = nextRetryAtUtc;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Gets open or failed invoice collectible via gateway.</summary>
    public bool IsOutstanding =>
        Status is BillingInvoiceStatus.Open or BillingInvoiceStatus.Failed;
}
