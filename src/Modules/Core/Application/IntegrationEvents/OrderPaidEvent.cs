using MediatR;
using System;
using System.Collections.Generic;

namespace Core.Application.IntegrationEvents;

public class OrderPaidEvent : INotification
{
    public Guid OrderId { get; }
    public IReadOnlyCollection<OrderPaidItem> Items { get; }

    public OrderPaidEvent(Guid orderId, IReadOnlyCollection<OrderPaidItem> items)
    {
        OrderId = orderId;
        Items = items;
    }
}

public class OrderPaidItem
{
    public Guid ProductId { get; }
    public decimal Quantity { get; }

    public OrderPaidItem(Guid productId, decimal quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}
