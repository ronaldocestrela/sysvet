using Core.Domain;
using Sales.Domain.Enums;
using Sales.Domain.ValueObjects;

namespace Sales.Domain.Entities;

/// <summary>
/// Snapshot line on a sales order (product or service).
/// </summary>
public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public OrderItemKind Kind { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? CatalogOfferId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;
    public Guid? PerformerUserId { get; private set; }
    public CommissionRole? PerformerRole { get; private set; }
    public decimal ReturnedQuantity { get; private set; }

    public Money TotalPrice => Money.CreateUnsafe(Quantity * UnitPrice.Amount);

    public decimal RemainingQuantity => Quantity - ReturnedQuantity;

    private OrderItem() { }

    internal OrderItem(
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        Guid? catalogOfferId,
        string productName,
        decimal quantity,
        decimal unitPrice,
        Guid? performerUserId = null,
        CommissionRole? performerRole = null)
        : this(Guid.NewGuid(), orderId, kind, productId, catalogOfferId, productName, quantity, unitPrice, performerUserId, performerRole)
    {
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        Guid? catalogOfferId,
        string productName,
        decimal quantity,
        decimal unitPrice,
        Guid? performerUserId,
        CommissionRole? performerRole)
        : base(id)
    {
        OrderId = orderId;
        Kind = kind;
        ProductId = productId;
        CatalogOfferId = catalogOfferId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = Money.CreateUnsafe(unitPrice);
        PerformerUserId = performerUserId;
        PerformerRole = performerRole;
    }

    /// <summary>Records returned quantity on a paid order.</summary>
    internal Result RecordReturn(decimal quantity)
    {
        if (quantity <= 0)
        {
            return Result.Failure(ErrorCodes.Order.InvalidQuantity);
        }

        if (quantity > RemainingQuantity)
        {
            return Result.Failure(ErrorCodes.Order.ReturnExceedsRemainingQuantity);
        }

        ReturnedQuantity += quantity;
        return Result.Success();
    }

    /// <summary>Rehydrates a line from sync pull.</summary>
    public static OrderItem Restore(
        Guid id,
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        Guid? catalogOfferId,
        string productName,
        decimal quantity,
        decimal unitPrice,
        Guid? performerUserId = null,
        CommissionRole? performerRole = null,
        decimal returnedQuantity = 0)
    {
        var item = new OrderItem(id, orderId, kind, productId, catalogOfferId, productName, quantity, unitPrice, performerUserId, performerRole)
        {
            ReturnedQuantity = returnedQuantity
        };
        return item;
    }
}
