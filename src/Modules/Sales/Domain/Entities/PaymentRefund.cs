using Core.Domain;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Partial or full reversal of a recorded payment slice (estorno).
/// </summary>
public class PaymentRefund : Entity
{
    public Guid PaymentId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public string? RefundNsu { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private PaymentRefund() { }

    internal static PaymentRefund Create(
        Guid paymentId,
        decimal amount,
        string? refundNsu,
        DateTimeOffset? createdAt = null)
    {
        var money = Money.CreateUnsafe(amount);
        return new PaymentRefund
        {
            PaymentId = paymentId,
            Amount = money,
            RefundNsu = string.IsNullOrWhiteSpace(refundNsu) ? null : refundNsu.Trim(),
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };
    }

    /// <summary>Rehydrates a refund from sync pull.</summary>
    public static PaymentRefund Restore(
        Guid id,
        Guid paymentId,
        decimal amount,
        string? refundNsu,
        DateTimeOffset createdAt)
        => new(id, paymentId, Money.CreateUnsafe(amount), refundNsu, createdAt);

    private PaymentRefund(Guid id, Guid paymentId, Money amount, string? refundNsu, DateTimeOffset createdAt)
        : base(id)
    {
        PaymentId = paymentId;
        Amount = amount;
        RefundNsu = refundNsu;
        CreatedAt = createdAt;
    }
}
