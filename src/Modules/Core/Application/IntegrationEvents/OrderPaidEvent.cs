using MediatR;

namespace Core.Application.IntegrationEvents;

/// <summary>
/// Published after a sale is paid, stock debited, and finance integration is pending.
/// </summary>
public class OrderPaidEvent : INotification
{
    public Guid OrderId { get; }
    public Guid? TutorId { get; }
    public decimal TotalAmount { get; }
    public string FinanceIntegrationStatus { get; }
    public IReadOnlyCollection<OrderPaidItem> Items { get; }
    public IReadOnlyCollection<OrderPaidPayment> Payments { get; }

    public OrderPaidEvent(
        Guid orderId,
        Guid? tutorId,
        decimal totalAmount,
        string financeIntegrationStatus,
        IReadOnlyCollection<OrderPaidItem> items,
        IReadOnlyCollection<OrderPaidPayment> payments)
    {
        OrderId = orderId;
        TutorId = tutorId;
        TotalAmount = totalAmount;
        FinanceIntegrationStatus = financeIntegrationStatus;
        Items = items;
        Payments = payments;
    }
}

/// <summary>Product line debited on pay.</summary>
public class OrderPaidItem
{
    public Guid? ProductId { get; }
    public decimal Quantity { get; }
    public string Kind { get; }

    public OrderPaidItem(Guid? productId, decimal quantity, string kind)
    {
        ProductId = productId;
        Quantity = quantity;
        Kind = kind;
    }
}

/// <summary>Payment slice recorded on the order.</summary>
public class OrderPaidPayment
{
    public string Method { get; }
    public decimal Amount { get; }

    public OrderPaidPayment(string method, decimal amount)
    {
        Method = method;
        Amount = amount;
    }
}
