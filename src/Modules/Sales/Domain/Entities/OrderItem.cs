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
    public string ProductName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;

    public Money TotalPrice => Money.CreateUnsafe(Quantity * UnitPrice.Amount);

    private OrderItem() { }

    internal OrderItem(
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        string productName,
        decimal quantity,
        decimal unitPrice)
        : this(Guid.NewGuid(), orderId, kind, productId, productName, quantity, unitPrice)
    {
    }

    private OrderItem(
        Guid id,
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        string productName,
        decimal quantity,
        decimal unitPrice)
        : base(id)
    {
        OrderId = orderId;
        Kind = kind;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = Money.CreateUnsafe(unitPrice);
    }

    /// <summary>Rehydrates a line from sync pull.</summary>
    public static OrderItem Restore(
        Guid id,
        Guid orderId,
        OrderItemKind kind,
        Guid? productId,
        string productName,
        decimal quantity,
        decimal unitPrice)
        => new(id, orderId, kind, productId, productName, quantity, unitPrice);
}
