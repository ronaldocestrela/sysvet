using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published after items are returned, stock restored, and refunds applied.
/// </summary>
public sealed class OrderReturnedEvent : INotification
{
    public Guid OrderId { get; }
    public Guid ReturnId { get; }
    public decimal RefundAmount { get; }
    public string OrderStatus { get; }

    public OrderReturnedEvent(Guid orderId, Guid returnId, decimal refundAmount, string orderStatus)
    {
        OrderId = orderId;
        ReturnId = returnId;
        RefundAmount = refundAmount;
        OrderStatus = orderStatus;
    }
}
