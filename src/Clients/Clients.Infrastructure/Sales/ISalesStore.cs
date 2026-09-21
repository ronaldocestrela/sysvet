using Clients.Infrastructure.Http;
using Core.Domain;

namespace Clients.Infrastructure.Sales;

/// <summary>Offline-first PDV store (SQLite + outbox).</summary>
public interface ISalesStore
{
    Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default);
    Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RecordCashMovementAsync(Guid cashRegisterId, string kind, decimal amount, string reason, CancellationToken cancellationToken = default);
    Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAndPayOrderAsync(CreateSalesOrderClientRequest request, IReadOnlyList<PayOrderPaymentClientDto> payments, CancellationToken cancellationToken = default);
    Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> RefundOrderPaymentAsync(Guid orderId, Guid paymentId, decimal amount, string? refundNsu = null, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ReturnOrderAsync(Guid orderId, Guid returnId, IReadOnlyList<ReturnOrderLineClientDto> lines, CancellationToken cancellationToken = default);
    Task<SalesOrderSyncState> GetOrderSyncStateAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProductKitClientDto>>> ListProductKitsAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ServicePackageClientDto>>> ListServicePackagesAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<PrepaidBalanceClientDto>>> ListPrepaidBalancesAsync(Guid? petId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ConsumePrepaidUseAsync(ConsumePrepaidUseClientRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Product kit row for offline PDV.</summary>
public sealed class ProductKitClientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
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
