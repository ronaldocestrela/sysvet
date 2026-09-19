using Clients.Infrastructure.Http;
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
}
