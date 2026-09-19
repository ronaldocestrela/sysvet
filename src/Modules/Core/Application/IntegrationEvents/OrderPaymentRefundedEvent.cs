using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published after a payment slice on a paid order is partially or fully refunded (estorno).
/// </summary>
public class OrderPaymentRefundedEvent : INotification
{
    public Guid OrderId { get; }
    public Guid PaymentId { get; }
    public Guid RefundId { get; }
    public string Method { get; }
    public decimal Amount { get; }
    public string? RefundNsu { get; }
    public string OrderStatus { get; }

    public OrderPaymentRefundedEvent(
        Guid orderId,
        Guid paymentId,
        Guid refundId,
        string method,
        decimal amount,
        string? refundNsu,
        string orderStatus)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        RefundId = refundId;
        Method = method;
        Amount = amount;
        RefundNsu = refundNsu;
        OrderStatus = orderStatus;
    }
}
