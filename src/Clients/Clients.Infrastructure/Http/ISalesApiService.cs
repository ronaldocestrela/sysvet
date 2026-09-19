using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>Online PDV and cash register API (Fase 6.1).</summary>
public interface ISalesApiService
{
    Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default);
    Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default);
    Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateOrderAsync(CreateSalesOrderClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> PayOrderAsync(Guid orderId, IReadOnlyList<PayOrderPaymentClientDto> payments, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RefundOrderPaymentAsync(Guid orderId, Guid paymentId, decimal amount, string? refundNsu = null, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CommissionRuleClientDto>>> ListCommissionRulesAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> UpsertCommissionRuleAsync(CommissionRuleUpsertClientRequest request, CancellationToken cancellationToken = default);
}

public sealed class CommissionRuleClientDto
{
    public Guid Id { get; init; }
    public string Role { get; init; } = string.Empty;
    public string AppliesTo { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
}

public sealed class CommissionRuleUpsertClientRequest
{
    public string Role { get; init; } = string.Empty;
    public string AppliesTo { get; init; } = string.Empty;
    public decimal RatePercent { get; init; }
}

public sealed class CreateSalesOrderClientRequest
{
    public Guid CashRegisterId { get; init; }
    public Guid? TutorId { get; init; }
    public Guid? PetId { get; init; }
    public Guid? SourceQuoteId { get; init; }
    public decimal DiscountPercent { get; init; }
    public List<SalesOrderItemClientDto> Items { get; init; } = new();
}

public sealed class CashRegisterClientDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public IReadOnlyList<CashRegisterMethodTotalsClientDto> MethodTotals { get; set; } =
        Array.Empty<CashRegisterMethodTotalsClientDto>();
}

public sealed class CashRegisterMethodTotalsClientDto
{
    public string Method { get; set; } = string.Empty;
    public decimal Gross { get; set; }
    public decimal Refunded { get; set; }
    public decimal Net => Gross - Refunded;
}

public sealed class SalesOrderItemClientDto
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = "Product";
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ReturnedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public sealed class PayOrderPaymentClientDto
{
    public Guid? Id { get; set; }
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
    public string? Nsu { get; set; }
    public string? AuthorizationCode { get; set; }
    public string? Provider { get; set; }
    public int Installments { get; set; } = 1;
    public decimal RemainingRefundable { get; set; }
    public IReadOnlyList<SalesOrderPaymentRefundClientDto> Refunds { get; set; } =
        Array.Empty<SalesOrderPaymentRefundClientDto>();
}

public sealed class SalesOrderPaymentRefundClientDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public string? RefundNsu { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed class SalesOrderDetailClientDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public Guid? TutorId { get; init; }
    public Guid? PetId { get; init; }
    public string FinanceIntegrationStatus { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<SalesOrderItemClientDto> Items { get; init; } = Array.Empty<SalesOrderItemClientDto>();
    public IReadOnlyList<PayOrderPaymentClientDto> Payments { get; init; } = Array.Empty<PayOrderPaymentClientDto>();
}
