using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>REST client for sales / PDV endpoints.</summary>
public sealed class SalesApiService : ISalesApiService
{
    private readonly ApiClient _apiClient;
    private readonly ISyncConnectivity _connectivity;

    public SalesApiService(ApiClient apiClient, ISyncConnectivity connectivity)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
    }

    public Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<object, Guid>(
            "/api/v1/sales/cash-registers/open",
            new { openingBalance },
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<bool>(OfflineError()));
        }

        return _apiClient.PostAsync<object, bool>(
            "/api/v1/sales/cash-registers/close",
            new { cashRegisterId, actualClosingBalance },
            cancellationToken: cancellationToken);
    }

    public Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<CashRegisterClientDto?>(OfflineError()));
        }

        return _apiClient.GetAsync<CashRegisterClientDto?>("/api/v1/sales/cash-registers/open", cancellationToken);
    }

    public Task<Result<Guid>> CreateOrderAsync(CreateSalesOrderClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<CreateSalesOrderClientRequest, Guid>(
            "/api/v1/sales/orders",
            request,
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public Task<Result<bool>> PayOrderAsync(Guid orderId, IReadOnlyList<PayOrderPaymentClientDto> payments, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<bool>(OfflineError()));
        }

        return _apiClient.PostAsync<object, bool>(
            $"/api/v1/sales/orders/{orderId}/pay",
            new { payments },
            idempotencyKey: Guid.NewGuid(),
            cancellationToken: cancellationToken);
    }

    public Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<SalesOrderDetailClientDto>(OfflineError()));
        }

        return _apiClient.GetAsync<SalesOrderDetailClientDto>($"/api/v1/sales/orders/{orderId}", cancellationToken);
    }

    private static Error OfflineError() =>
        new("Sales.Offline", "PDV requer conexão com a internet nesta versão.");
}
