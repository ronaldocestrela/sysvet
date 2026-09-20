using Clients.Infrastructure.Http;
using Clients.Infrastructure.Sales;
using Core.Domain;

namespace SharedUI.Services;

/// <summary>Fallback mock when API is not wired (tests / offline demos).</summary>
public class MockSalesApiService : ISalesApiService
{
    private CashRegisterClientDto? _openRegister;

    public Task<Result<Guid>> OpenCashRegisterAsync(decimal openingBalance, CancellationToken cancellationToken = default)
    {
        _openRegister = new CashRegisterClientDto
        {
            Id = Guid.NewGuid(),
            Status = "Open",
            OpeningBalance = openingBalance,
            CurrentBalance = openingBalance
        };
        return Task.FromResult(Result.Success(_openRegister.Id));
    }

    public Task<Result<bool>> CloseCashRegisterAsync(Guid cashRegisterId, decimal actualClosingBalance, CancellationToken cancellationToken = default)
    {
        if (_openRegister != null && _openRegister.Id == cashRegisterId)
        {
            _openRegister = null;
            return Task.FromResult(Result.Success(true));
        }

        return Task.FromResult(Result.Failure<bool>(new Error("Mock", "Caixa não encontrado.")));
    }

    public Task<Result<CashRegisterClientDto?>> GetOpenCashRegisterAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(_openRegister));

    public Task<Result<Guid>> CreateOrderAsync(CreateSalesOrderClientRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(Guid.NewGuid()));

    public Task<Result<bool>> PayOrderAsync(Guid orderId, IReadOnlyList<PayOrderPaymentClientDto> payments, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(true));

    public Task<Result<Guid>> RefundOrderPaymentAsync(Guid orderId, Guid paymentId, decimal amount, string? refundNsu = null, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(Guid.NewGuid()));

    public Task<Result<SalesOrderDetailClientDto>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(new SalesOrderDetailClientDto { Id = orderId, Status = "Paid", TotalAmount = 0 }));

    public Task<Result<IReadOnlyList<CommissionRuleClientDto>>> ListCommissionRulesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<IReadOnlyList<CommissionRuleClientDto>>(Array.Empty<CommissionRuleClientDto>()));

    public Task<Result<Guid>> UpsertCommissionRuleAsync(CommissionRuleUpsertClientRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(Guid.NewGuid()));

    public Task<Result<IReadOnlyList<ProductKitClientDto>>> ListProductKitsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<IReadOnlyList<ProductKitClientDto>>(Array.Empty<ProductKitClientDto>()));

    public Task<Result<IReadOnlyList<ServicePackageClientDto>>> ListServicePackagesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<IReadOnlyList<ServicePackageClientDto>>(Array.Empty<ServicePackageClientDto>()));

    public Task<Result<IReadOnlyList<PrepaidBalanceClientDto>>> ListPrepaidBalancesAsync(Guid? petId, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<IReadOnlyList<PrepaidBalanceClientDto>>(Array.Empty<PrepaidBalanceClientDto>()));

    public Task<Result<bool>> ConsumePrepaidUseAsync(ConsumePrepaidUseClientRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(true));
}
