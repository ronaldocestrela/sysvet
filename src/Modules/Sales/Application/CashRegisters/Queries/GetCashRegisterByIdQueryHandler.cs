using Core.Domain;
using MediatR;
using Sales.Application.CashRegisters.Dtos;
using Sales.Domain.Repositories;

namespace Sales.Application.CashRegisters.Queries;

public sealed class GetCashRegisterByIdQueryHandler : IRequestHandler<GetCashRegisterByIdQuery, Result<CashRegisterDetailDto?>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IOrderRepository _orderRepository;

    public GetCashRegisterByIdQueryHandler(
        ICashRegisterRepository cashRegisterRepository,
        IOrderRepository orderRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<CashRegisterDetailDto?>> Handle(GetCashRegisterByIdQuery request, CancellationToken cancellationToken)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (register is null)
        {
            return Result.Success<CashRegisterDetailDto?>(null);
        }

        var totals = await _orderRepository.GetPaymentTotalsForCashRegisterAsync(register.Id, cancellationToken);
        return Result.Success<CashRegisterDetailDto?>(CashRegisterDtoMapper.ToDetail(register, totals));
    }
}
