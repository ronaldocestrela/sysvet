using Core.Domain;
using Sales.Domain.Enums;

namespace Sales.Domain.Entities;

/// <summary>
/// Tenant-scoped commission rate for a professional role and line kind.
/// </summary>
public sealed class CommissionRule : AggregateRoot
{
    public CommissionRole Role { get; private set; }
    public CommissionAppliesTo AppliesTo { get; private set; }
    public decimal RatePercent { get; private set; }

    private CommissionRule() { }

    private CommissionRule(Guid id, CommissionRole role, CommissionAppliesTo appliesTo, decimal ratePercent)
        : base(id)
    {
        Role = role;
        AppliesTo = appliesTo;
        RatePercent = ratePercent;
    }

    /// <summary>Creates or replaces a rule for the unique role/applies-to pair.</summary>
    public static Result<CommissionRule> Create(CommissionRole role, CommissionAppliesTo appliesTo, decimal ratePercent)
        => Create(Guid.NewGuid(), role, appliesTo, ratePercent);

    /// <summary>Creates a rule with a known id (sync).</summary>
    public static Result<CommissionRule> Create(Guid id, CommissionRole role, CommissionAppliesTo appliesTo, decimal ratePercent)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<CommissionRule>(ErrorCodes.CommissionRule.InvalidId);
        }

        if (ratePercent < 0 || ratePercent > 100)
        {
            return Result.Failure<CommissionRule>(ErrorCodes.CommissionRule.InvalidRate);
        }

        return Result.Success(new CommissionRule(id, role, appliesTo, ratePercent));
    }

    /// <summary>Updates the rate while keeping role and scope.</summary>
    public Result SetRate(decimal ratePercent)
    {
        if (ratePercent < 0 || ratePercent > 100)
        {
            return Result.Failure(ErrorCodes.CommissionRule.InvalidRate);
        }

        RatePercent = ratePercent;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
