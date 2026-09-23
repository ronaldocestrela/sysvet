using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Proration row pending gateway charge (9.4).</summary>
public sealed class SubscriptionAdjustment : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Signed amount: positive charge, negative credit issued.</summary>
    public decimal Amount { get; private set; }

    /// <summary>Optional previous plan id.</summary>
    public Guid? FromPlanId { get; private set; }

    /// <summary>Optional new plan id.</summary>
    public Guid? ToPlanId { get; private set; }

    /// <summary>Optional add-on id for add-on proration.</summary>
    public Guid? AddOnId { get; private set; }

    /// <summary>Billing pipeline status.</summary>
    public AdjustmentStatus Status { get; private set; }

    /// <summary>Invoice that included this adjustment, when invoiced.</summary>
    public Guid? BillingInvoiceId { get; private set; }

#pragma warning disable CS8618
    private SubscriptionAdjustment()
    {
    }
#pragma warning restore CS8618

    /// <summary>Records a pending billing adjustment.</summary>
    public static Result<SubscriptionAdjustment> CreatePending(
        Guid tenantId,
        decimal amount,
        Guid? fromPlanId,
        Guid? toPlanId,
        Guid? addOnId = null)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<SubscriptionAdjustment>(ErrorCodes.Subscription.InvalidReference);
        }

        return Result.Success(new SubscriptionAdjustment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Amount = amount,
            FromPlanId = fromPlanId,
            ToPlanId = toPlanId,
            AddOnId = addOnId,
            Status = AdjustmentStatus.PendingBilling,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Links adjustment to an open invoice.</summary>
    public Result MarkInvoiced(Guid invoiceId)
    {
        if (Status is not AdjustmentStatus.PendingBilling)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        if (invoiceId == Guid.Empty)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidTenant);
        }

        Status = AdjustmentStatus.Invoiced;
        BillingInvoiceId = invoiceId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    /// <summary>Marks adjustment settled after invoice payment.</summary>
    public Result MarkSettled()
    {
        if (Status is not AdjustmentStatus.Invoiced)
        {
            return Result.Failure(ErrorCodes.Billing.InvalidInvoiceTransition);
        }

        Status = AdjustmentStatus.Settled;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
