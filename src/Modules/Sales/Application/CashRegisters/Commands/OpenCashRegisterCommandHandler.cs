using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Sales.Application.CashRegisters.Commands;

public class OpenCashRegisterCommandHandler : IRequestHandler<OpenCashRegisterCommand, Result<Guid>>
{
    private readonly ICashRegisterRepository _cashRegisterRepository;
    private readonly ITenantContext _tenantContext;

    public OpenCashRegisterCommandHandler(ICashRegisterRepository cashRegisterRepository, ITenantContext tenantContext)
    {
        _cashRegisterRepository = cashRegisterRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(OpenCashRegisterCommand request, CancellationToken cancellationToken)
    {
        var existingOpenRegister = await _cashRegisterRepository.GetOpenCashRegisterByUserAsync(_tenantContext.UserId, cancellationToken);
        if (existingOpenRegister != null)
        {
            return Result.Failure<Guid>(new Error("CashRegister.AlreadyOpen", "O usuário já possui um caixa aberto."));
        }

        var cashRegisterResult = CashRegister.Open(_tenantContext.UserId, request.OpeningBalance);
        if (!cashRegisterResult.IsSuccess)
        {
            return Result.Failure<Guid>(cashRegisterResult.Error);
        }

        var cashRegister = cashRegisterResult.Value;
        _cashRegisterRepository.Add(cashRegister);
        
        return Result.Success(cashRegister.Id);
    }
}
