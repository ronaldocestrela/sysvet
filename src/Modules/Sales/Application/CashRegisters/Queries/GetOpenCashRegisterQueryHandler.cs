using Core.Domain;
using MediatR;
using Sales.Application.CashRegisters.Dtos;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.CashRegisters.Queries;

public sealed class GetOpenCashRegisterQueryHandler : IRequestHandler<GetOpenCashRegisterQuery, Result<OpenCashRegisterDto?>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantContext _tenantContext;

    public GetOpenCashRegisterQueryHandler(
        ICashRegisterRepository cashRegisterRepository,
        IOrderRepository orderRepository,
        ITenantContext tenantContext)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _orderRepository = orderRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<OpenCashRegisterDto?>> Handle(GetOpenCashRegisterQuery request, CancellationToken cancellationToken)
    {
        var register = await _cashRegisterRepository.GetOpenCashRegisterByUserAsync(_tenantContext.UserId, cancellationToken);
        if (register is null)
        {
            return Result.Success<OpenCashRegisterDto?>(null);
        }

        var totals = await _orderRepository.GetPaymentTotalsForCashRegisterAsync(register.Id, cancellationToken);
        var methodTotals = totals
            .Select(t => new CashRegisterMethodTotalsDto
            {
                Method = t.Method,
                Gross = t.Gross,
                Refunded = t.Refunded
            })
            .ToList();

        var cashNet = methodTotals.FirstOrDefault(m => m.Method == PaymentMethod.Cash)?.Net ?? 0m;
        var current = register.OpeningBalance.Amount + cashNet;

        return Result.Success<OpenCashRegisterDto?>(new OpenCashRegisterDto
        {
            Id = register.Id,
            Status = register.Status.ToString(),
            OpeningBalance = register.OpeningBalance.Amount,
            CurrentBalance = current,
            MethodTotals = methodTotals
        });
    }
}
