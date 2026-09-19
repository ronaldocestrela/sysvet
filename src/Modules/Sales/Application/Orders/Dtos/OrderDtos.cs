using Sales.Domain.Enums;

namespace Sales.Application.Orders.Dtos;

/// <summary>Sales order detail for receipt and audit.</summary>
public sealed class OrderDetailDto
{
    public Guid Id { get; init; }
    public Guid CashRegisterId { get; init; }
    public OrderStatus Status { get; init; }
    public Guid? TutorId { get; init; }
    public Guid? PetId { get; init; }
    public Guid? SourceQuoteId { get; init; }
    public FinanceIntegrationStatus FinanceIntegrationStatus { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<OrderItemDetailDto> Items { get; init; } = Array.Empty<OrderItemDetailDto>();
    public IReadOnlyList<OrderPaymentDetailDto> Payments { get; init; } = Array.Empty<OrderPaymentDetailDto>();
}

public sealed class OrderItemDetailDto
{
    public OrderItemKind Kind { get; init; }
    public Guid? ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

public sealed class OrderPaymentDetailDto
{
    public Guid Id { get; init; }
    public PaymentMethod Method { get; init; }
    public decimal Amount { get; init; }
    public string? Nsu { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? Provider { get; init; }
    public int Installments { get; init; }
    public decimal RemainingRefundable { get; init; }
    public IReadOnlyList<OrderPaymentRefundDetailDto> Refunds { get; init; } =
        Array.Empty<OrderPaymentRefundDetailDto>();
}

public sealed class OrderPaymentRefundDetailDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? RefundNsu { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
