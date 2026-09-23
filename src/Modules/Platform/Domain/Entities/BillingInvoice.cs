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

        if (Status is not BillingInvoiceStatus.Open)
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
        UpdatedAt = DateTimeOffset.UtcNow;
        return charge;
    }
}
