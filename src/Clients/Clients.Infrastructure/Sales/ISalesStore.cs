using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Sales;

/// <summary>Offline-first PDV store (SQLite + outbox).</summary>
public interface ISalesStore
{
    Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default);
    Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default);
    Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAndPayOrderAsync(CreateSalesOrderClientRequest request, IReadOnlyList<PayOrderPaymentClientDto> payments, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RefundOrderPaymentAsync(Guid orderId, Guid paymentId, decimal amount, string? refundNsu = null, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ReturnOrderAsync(Guid orderId, Guid returnId, IReadOnlyList<ReturnOrderLineClientDto> lines, CancellationToken cancellationToken = default);
    Task<SalesOrderSyncState> GetOrderSyncStateAsync(Guid orderId, CancellationToken cancellationToken = default);
}

/// <summary>Outbox sync state for a local paid order.</summary>
public sealed class ReturnOrderLineClientDto
{
    public Guid OrderItemId { get; init; }
    public decimal Quantity { get; init; }
}

public enum SalesOrderSyncState
{
    Synced,
    Pending,
    Conflict
}
