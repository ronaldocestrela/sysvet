using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Frozen commission amount earned on a paid order line.
/// </summary>
public sealed class CommissionAccrual : Entity
{
    public Guid OrderId { get; internal set; }
    public Guid OrderItemId { get; private set; }
    public Guid PayeeUserId { get; private set; }
    public CommissionRole Role { get; private set; }
    public decimal RatePercent { get; private set; }
    public Money BaseAmount { get; private set; } = Money.Zero;
    public Money CommissionAmount { get; private set; } = Money.Zero;
    public CommissionAccrualStatus Status { get; private set; } = CommissionAccrualStatus.Accrued;

    private CommissionAccrual() { }

    internal CommissionAccrual(
        Guid id,
        Guid orderId,
        Guid orderItemId,
        Guid payeeUserId,
        CommissionRole role,
        decimal ratePercent,
        decimal baseAmount,
        decimal commissionAmount)
        : base(id)
    {
        OrderId = orderId;
        OrderItemId = orderItemId;
        PayeeUserId = payeeUserId;
        Role = role;
        RatePercent = ratePercent;
        BaseAmount = Money.CreateUnsafe(baseAmount);
        CommissionAmount = Money.CreateUnsafe(commissionAmount);
    }

    /// <summary>Marks the accrual reversed after a proportional sale return.</summary>
    public Result Reverse(decimal reverseAmount)
    {
        if (Status == CommissionAccrualStatus.Reversed)
        {
            return Result.Success();
        }

        if (reverseAmount < 0 || reverseAmount > CommissionAmount.Amount)
        {
            return Result.Failure(ErrorCodes.Commission.InvalidReverseAmount);
        }

        if (reverseAmount >= CommissionAmount.Amount)
        {
            Status = CommissionAccrualStatus.Reversed;
        }
        else
        {
            CommissionAmount = Money.CreateUnsafe(CommissionAmount.Amount - reverseAmount);
        }

        return Result.Success();
    }

    /// <summary>Rehydrates from persistence or sync.</summary>
    public static CommissionAccrual Restore(
        Guid id,
        Guid orderId,
        Guid orderItemId,
        Guid payeeUserId,
        CommissionRole role,
        decimal ratePercent,
        decimal baseAmount,
        decimal commissionAmount,
        CommissionAccrualStatus status)
        => new(id, orderId, orderItemId, payeeUserId, role, ratePercent, baseAmount, commissionAmount)
        {
            Status = status
        };
}
