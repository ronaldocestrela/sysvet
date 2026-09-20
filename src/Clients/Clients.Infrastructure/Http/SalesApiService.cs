using Clients.Infrastructure.Sales;
using Clients.Infrastructure.Sync;
using Core.Domain;

namespace Clients.Infrastructure.Http;

/// <summary>REST client for sales / PDV endpoints.</summary>
public sealed class SalesApiService : ISalesApiService
{
    private readonly ApiClient _apiClient;
    private readonly ISyncConnectivity _connectivity;
    private readonly ISalesStore _salesStore;

    public SalesApiService(ApiClient apiClient, ISyncConnectivity connectivity, ISalesStore salesStore)
    {
        _apiClient = apiClient;
        _connectivity = connectivity;
        _salesStore = salesStore;
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

    public Task<Result<Guid>> RefundOrderPaymentAsync(
        Guid orderId,
        Guid paymentId,
        decimal amount,
        string? refundNsu = null,
        CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PostAsync<object, Guid>(
            $"/api/v1/sales/orders/{orderId}/payments/{paymentId}/refund",
            new { amount, refundNsu },
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

    public Task<Result<IReadOnlyList<CommissionRuleClientDto>>> ListCommissionRulesAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<CommissionRuleClientDto>>(OfflineError()));
        }

        return _apiClient.GetAsync<IReadOnlyList<CommissionRuleClientDto>>("/api/v1/sales/commission-rules", cancellationToken);
    }

    public Task<Result<Guid>> UpsertCommissionRuleAsync(CommissionRuleUpsertClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return Task.FromResult(Result.Failure<Guid>(OfflineError()));
        }

        return _apiClient.PutAsync<CommissionRuleUpsertClientRequest, Guid>(
            "/api/v1/sales/commission-rules",
            request,
            cancellationToken: cancellationToken);
    }

    public Task<Result<IReadOnlyList<ProductKitClientDto>>> ListProductKitsAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return _salesStore.ListProductKitsAsync(cancellationToken);
        }

        return _apiClient.GetAsync<IReadOnlyList<ProductKitClientDto>>("/api/v1/sales/product-kits", cancellationToken);
    }

    public Task<Result<IReadOnlyList<ServicePackageClientDto>>> ListServicePackagesAsync(CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return _salesStore.ListServicePackagesAsync(cancellationToken);
        }

        return _apiClient.GetAsync<IReadOnlyList<ServicePackageClientDto>>("/api/v1/sales/service-packages", cancellationToken);
    }

    public Task<Result<IReadOnlyList<PrepaidBalanceClientDto>>> ListPrepaidBalancesAsync(Guid? petId, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return _salesStore.ListPrepaidBalancesAsync(petId, cancellationToken);
        }

        var query = petId.HasValue ? $"?petId={petId}" : string.Empty;
        return _apiClient.GetAsync<IReadOnlyList<PrepaidBalanceClientDto>>($"/api/v1/sales/prepaid-balances{query}", cancellationToken);
    }

    public Task<Result<bool>> ConsumePrepaidUseAsync(ConsumePrepaidUseClientRequest request, CancellationToken cancellationToken = default)
    {
        if (!_connectivity.IsOnline)
        {
            return _salesStore.ConsumePrepaidUseAsync(request, cancellationToken);
        }

        return _apiClient.PostAsync<ConsumePrepaidUseClientRequest, bool>(
            "/api/v1/sales/prepaid-balances/consume",
            request,
            idempotencyKey: request.UsageId,
            cancellationToken: cancellationToken);
    }

    private static Error OfflineError() =>
        new("Sales.Offline", "PDV requer conexão com a internet nesta versão.");
}
