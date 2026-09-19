using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Payment slice recorded when an order is paid (split payments supported).
/// </summary>
public class Payment : Entity
{
    public Guid OrderId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;

    private Payment() { }

    private Payment(Guid orderId, PaymentMethod method, Money amount)
    {
        OrderId = orderId;
        Method = method;
        Amount = amount;
    }

    /// <summary>
    /// Creates a payment line with a non-negative amount.
    /// </summary>
    public static Result<Payment> Create(PaymentMethod method, decimal amount)
    {
        var moneyResult = Money.Create(amount);
        if (moneyResult.IsFailure)
        {
            return Result.Failure<Payment>(moneyResult.Error);
        }

        if (moneyResult.Value.Amount == 0)
        {
            return Result.Failure<Payment>(ErrorCodes.Payment.ZeroAmount);
        }

        return Result.Success(new Payment(Guid.Empty, method, moneyResult.Value));
    }

    internal static Payment Attach(Guid orderId, PaymentMethod method, Money amount)
    {
        return new Payment(orderId, method, amount);
    }

    /// <summary>Rehydrates a payment from sync pull.</summary>
    public static Payment Restore(Guid id, Guid orderId, PaymentMethod method, decimal amount)
        => new(id, orderId, method, Money.CreateUnsafe(amount));

    private Payment(Guid id, Guid orderId, PaymentMethod method, Money amount)
        : base(id)
    {
        OrderId = orderId;
        Method = method;
        Amount = amount;
    }
}
