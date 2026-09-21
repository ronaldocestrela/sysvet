using System.Linq;
using Core.Domain;
using MediatR;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.CashRegisters.Commands;

public sealed class RecordCashMovementCommandHandler : IRequestHandler<RecordCashMovementCommand, Result<Guid>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly IOrderRepository _orderRepository;

    public RecordCashMovementCommandHandler(
        ICashRegisterRepository cashRegisterRepository,
        IOrderRepository orderRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _orderRepository = orderRepository;
    }

    public async Task<Result<Guid>> Handle(RecordCashMovementCommand request, CancellationToken cancellationToken)
    {
        var register = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (register is null)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.CashRegister.NotFound);
        }

        var totals = await _orderRepository.GetPaymentTotalsForCashRegisterAsync(register.Id, cancellationToken);
        var cashNet = CashRegisterCashNet.FromTotals(totals);

        var result = request.Kind switch
        {
            CashMovementKind.Drop => register.RecordDrop(request.Amount, request.Reason, cashNet, request.MovementId),
            CashMovementKind.Supply => register.RecordSupply(request.Amount, request.Reason, request.MovementId),
            _ => Result.Failure<Guid>(Sales.Domain.ErrorCodes.CashRegister.MovementNotAllowed)
        };

        if (result.IsFailure)
        {
            return result;
        }

        var movement = register.Movements.First(m => m.Id == result.Value);
        _cashRegisterRepository.AddMovement(movement);
        return result;
    }
}
