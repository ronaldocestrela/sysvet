using Core.Domain;
using Sales.Domain.ValueObjects;
using System;

namespace Sales.Domain.Entities;

public class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;
    
    public Money TotalPrice => Money.CreateUnsafe(Quantity * UnitPrice.Amount);

    private OrderItem() { } // For EF Core

    internal OrderItem(Guid orderId, Guid productId, string productName, decimal quantity, decimal unitPrice)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = Money.CreateUnsafe(unitPrice);
    }
}
