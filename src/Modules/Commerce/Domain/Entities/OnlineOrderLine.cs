using Commerce.Domain.ValueObjects;
using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// Line item on an online order with price snapshot at order time.
/// </summary>
public sealed class OnlineOrderLine : Entity
{
    public Guid OnlineOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid ProductOfferId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;

#pragma warning disable CS8618
    private OnlineOrderLine() { }
#pragma warning restore CS8618

    internal static Result<OnlineOrderLine> Create(
        Guid orderId,
        Guid productId,
        Guid offerId,
        string productName,
        string sku,
        decimal quantity,
        Money unitPrice)
    {
        if (quantity <= 0)
        {
            return Result.Failure<OnlineOrderLine>(ErrorCodes.Order.Empty);
        }

        return Result.Success(new OnlineOrderLine
        {
            Id = Guid.NewGuid(),
            OnlineOrderId = orderId,
            ProductId = productId,
            ProductOfferId = offerId,
            ProductName = productName.Trim(),
            Sku = sku.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice
        });
    }
}
