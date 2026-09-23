using Core.Domain;
using Platform.Domain.Services;

namespace Platform.Domain.Entities;

/// <summary>Tenant subscription binding to a catalog plan (dbo).</summary>
public sealed class TenantSubscription : Entity
{
    /// <summary>Owning tenant.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Current plan id.</summary>
    public Guid PlanId { get; private set; }

    /// <summary>Subscription lifecycle status.</summary>
    public SubscriptionStatus Status { get; private set; }

    /// <summary>Start of current billing period (UTC).</summary>
    public DateTimeOffset PeriodStart { get; private set; }

    /// <summary>End of current billing period (UTC).</summary>
    public DateTimeOffset PeriodEnd { get; private set; }

    /// <summary>When trial ends; null when not on trial.</summary>
    public DateTimeOffset? TrialEndsAt { get; private set; }

    /// <summary>Action applied when trial ends.</summary>
    public TrialEndAction TrialEndAction { get; private set; } = TrialEndAction.Block;

    /// <summary>Credit balance from downgrades (applied in 9.4).</summary>
    public decimal CreditBalance { get; private set; }

    /// <summary>Active add-on links.</summary>
    public ICollection<TenantAddOn> AddOns { get; private set; } = new List<TenantAddOn>();

    /// <summary>Navigation to plan.</summary>
    public Plan? Plan { get; private set; }

#pragma warning disable CS8618
    private TenantSubscription()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates an active subscription without trial.</summary>
    public static Result<TenantSubscription> CreateActive(Guid tenantId, Guid planId, DateTimeOffset periodStartUtc)
    {
        if (tenantId == Guid.Empty || planId == Guid.Empty)
        {
            return Result.Failure<TenantSubscription>(ErrorCodes.Subscription.InvalidReference);
        }

        var periodEnd = periodStartUtc.AddDays(ProrationCalculator.DefaultPeriodDays);
        return Result.Success(new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            PeriodStart = periodStartUtc,
            PeriodEnd = periodEnd,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Creates a trial subscription.</summary>
    public static Result<TenantSubscription> CreateTrial(
        Guid tenantId,
        Guid planId,
        DateTimeOffset periodStartUtc,
        int trialDays,
        TrialEndAction endAction)
    {
        if (tenantId == Guid.Empty || planId == Guid.Empty)
        {
            return Result.Failure<TenantSubscription>(ErrorCodes.Subscription.InvalidReference);
        }

        if (trialDays < 0)
        {
            return Result.Failure<TenantSubscription>(ErrorCodes.Subscription.InvalidTrialDays);
        }

        var periodEnd = periodStartUtc.AddDays(ProrationCalculator.DefaultPeriodDays);
        var trialEnds = trialDays > 0 ? periodStartUtc.AddDays(trialDays) : (DateTimeOffset?)null;

        return Result.Success(new TenantSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            Status = trialEnds is not null ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
            PeriodStart = periodStartUtc,
            PeriodEnd = periodEnd,
            TrialEndsAt = trialEnds,
            TrialEndAction = endAction,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        });
    }

    /// <summary>Changes plan mid-cycle and returns proration delta before credit application.</summary>
    public Result<decimal> ChangePlan(Guid newPlanId, decimal oldPlanPrice, decimal newPlanPrice, DateTimeOffset asOfUtc)
    {
        if (Status == SubscriptionStatus.TrialExpired)
        {
            return Result.Failure<decimal>(ErrorCodes.Subscription.TrialExpired);
        }

        if (newPlanId == Guid.Empty)
        {
            return Result.Failure<decimal>(ErrorCodes.Subscription.InvalidReference);
        }

        var daysRemaining = ProrationCalculator.DaysRemaining(PeriodStart, PeriodEnd, asOfUtc);
        var delta = ProrationCalculator.CalculateDelta(oldPlanPrice, newPlanPrice, daysRemaining);
        ApplyCredit(delta, out var chargeableDelta);

        PlanId = newPlanId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(chargeableDelta);
    }

    /// <summary>Applies trial expiry according to configured end action.</summary>
    public Result<decimal> ExpireTrial(DateTimeOffset asOfUtc)
    {
        if (Status != SubscriptionStatus.Trial || TrialEndsAt is null || asOfUtc < TrialEndsAt)
        {
            return Result.Failure<decimal>(ErrorCodes.Subscription.TrialNotDue);
        }

        if (TrialEndAction == TrialEndAction.Block)
        {
            Status = SubscriptionStatus.TrialExpired;
            UpdatedAt = DateTimeOffset.UtcNow;
            return Result.Success(0m);
        }

        Status = SubscriptionStatus.Active;
        TrialEndsAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(0m);
    }

    /// <summary>Records add-on price delta and applies credit rules.</summary>
    public Result<decimal> ApplyAddOnPriceDelta(decimal signedDelta)
    {
        if (Status == SubscriptionStatus.TrialExpired)
        {
            return Result.Failure<decimal>(ErrorCodes.Subscription.TrialExpired);
        }

        ApplyCredit(signedDelta, out var chargeableDelta);
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success(chargeableDelta);
    }

    private void ApplyCredit(decimal delta, out decimal chargeableDelta)
    {
        if (delta < 0)
        {
            CreditBalance += -delta;
            chargeableDelta = 0;
            return;
        }

        var applied = Math.Min(CreditBalance, delta);
        CreditBalance -= applied;
        chargeableDelta = delta - applied;
    }
}
