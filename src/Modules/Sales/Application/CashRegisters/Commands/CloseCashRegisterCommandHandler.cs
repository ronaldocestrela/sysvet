using Core.Domain;
using MediatR;
using Sales.Domain.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Application.CashRegisters.Commands;

public class CloseCashRegisterCommandHandler : IRequestHandler<CloseCashRegisterCommand, Result<bool>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;

    public CloseCashRegisterCommandHandler(ICashRegisterRepository cashRegisterRepository)
    {
        _cashRegisterRepository = cashRegisterRepository;
    }

    public async Task<Result<bool>> Handle(CloseCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var cashRegister = await _cashRegisterRepository.GetByIdAsync(request.CashRegisterId, cancellationToken);
        if (cashRegister == null)
        {
            return Result.Failure<bool>(new Error("CashRegister.NotFound", "Caixa não encontrado."));
        }

        var result = cashRegister.Close(request.ActualClosingBalance);
        if (!result.IsSuccess)
        {
            return result;
        }

        _cashRegisterRepository.Update(cashRegister);
        
        return Result.Success(true);
    }
}
