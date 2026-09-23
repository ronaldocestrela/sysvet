using Core.Domain;

namespace Platform.Domain.Entities;

/// <summary>Sent or scheduled dunning notice for idempotency (9.5).</summary>
public sealed class DunningNotice : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Invoice referenced in the notice.</summary>
    public Guid InvoiceId { get; private set; }

    /// <summary>Day offset from past-due start (0, 3, 7, …).</summary>
    public int StepDay { get; private set; }

    /// <summary>Delivery channel.</summary>
    public DunningChannel Channel { get; private set; }

    /// <summary>When the notice was scheduled.</summary>
    public DateTimeOffset ScheduledAt { get; private set; }

    /// <summary>When delivery completed; null while pending send.</summary>
    public DateTimeOffset? SentAt { get; private set; }

#pragma warning disable CS8618
    private DunningNotice()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a pending notice row.</summary>
    public static Result<DunningNotice> Schedule(
        Guid tenantId,
        Guid invoiceId,
        int stepDay,
        DunningChannel channel,
        DateTimeOffset scheduledAt)
    {
        if (tenantId == Guid.Empty || invoiceId == Guid.Empty)
        {
            return Result.Failure<DunningNotice>(ErrorCodes.Billing.InvalidTenant);
        }

        if (stepDay < 0)
        {
            return Result.Failure<DunningNotice>(ErrorCodes.Dunning.InvalidStep);
        }

        return Result.Success(new DunningNotice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceId = invoiceId,
            StepDay = stepDay,
            Channel = channel,
            ScheduledAt = scheduledAt,
            UpdatedAt = scheduledAt,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Marks notice as sent.</summary>
    public Result MarkSent(DateTimeOffset sentAtUtc)
    {
        if (SentAt is not null)
        {
            return Result.Success();
        }

        SentAt = sentAtUtc;
        UpdatedAt = sentAtUtc;
        return Result.Success();
    }

    /// <summary>Unique key for idempotent scheduling.</summary>
    public string IdempotencyKey => $"{TenantId:N}:{InvoiceId:N}:{StepDay}:{(int)Channel}";
}
