using Core.Domain;
using MediatR;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.CashRegisters.Commands;

public class CloseCashRegisterCommandHandler : IRequestHandler<CloseCashRegisterCommand, Result<bool>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IOrderRepository _orderRepository;

    public CloseCashRegisterCommandHandler(
        ICashRegisterRepository cashRegisterRepository,
        IOrderRepository orderRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<bool>> Handle(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var cashRegister = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (cashRegister == null)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.CashRegister.NotFound);
        }

        if (cashRegister.Status == CashRegisterStatus.Closed)
        {
            return Result.Success(true);
        }

        var totals = await _orderRepository.GetPaymentTotalsForCashRegisterAsync(cashRegister.Id, cancellationToken);
        var cashNet = CashRegisterCashNet.FromTotals(totals);
        var result = cashRegister.Close(request.ActualClosingBalance, cashNet);
        if (!result.IsSuccess)
        {
            return result;
        }

        _cashRegisterRepository.Update(cashRegister);

        return Result.Success(true);
    }
}
