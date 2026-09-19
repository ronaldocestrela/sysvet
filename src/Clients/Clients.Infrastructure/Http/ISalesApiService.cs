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
    Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public sealed class CreateSalesOrderClientRequest
{
    public Guid CashRegisterId { get; init; }
    public Guid? TutorId { get; init; }
    public Guid? PetId { get; init; }
    public Guid? SourceQuoteId { get; init; }
    public List<SalesOrderItemClientDto> Items { get; init; } = new();
}

public sealed class CashRegisterClientDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
}

public sealed class SalesOrderItemClientDto
{
    public string Kind { get; set; } = "Product";
    public Guid? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class PayOrderPaymentClientDto
{
    public string Method { get; set; } = "Cash";
    public decimal Amount { get; set; }
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
