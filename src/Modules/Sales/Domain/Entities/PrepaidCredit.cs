using Core.Domain;

namespace Sales.Domain.Entities;

/// <summary>Uses credited from a paid package order line (supports partial reversal on return).</summary>
public sealed class PrepaidCredit : Entity
{
    public Guid PrepaidBalanceId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid OrderItemId { get; private set; }
    public int UsesCredited { get; private set; }
    public int UsesRemaining { get; private set; }

    private PrepaidCredit() { }

    internal PrepaidCredit(Guid id, Guid balanceId, Guid orderId, Guid orderItemId, int uses)
        : base(id)
    {
        PrepaidBalanceId = balanceId;
        OrderId = orderId;
        OrderItemId = orderItemId;
        UsesCredited = uses;
        UsesRemaining = uses;
    }

    /// <summary>Reverses unused portion of this credit (return flow).</summary>
    internal Result Reverse(int usesToReverse)
    {
        if (usesToReverse <= 0)
        {
            return Result.Success();
        }

        if (usesToReverse > UsesRemaining)
        {
            return Result.Failure(ErrorCodes.Package.ReturnAfterConsumption);
        }

        UsesRemaining -= usesToReverse;
        return Result.Success();
    }

    internal void ConsumeOne()
    {
        if (UsesRemaining > 0)
        {
            UsesRemaining--;
        }
    }
}
