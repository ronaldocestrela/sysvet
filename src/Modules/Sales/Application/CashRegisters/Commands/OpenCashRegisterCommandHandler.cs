using Core.Domain;
using MediatR;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

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
        if (request.CashRegisterId is Guid clientId && clientId != Guid.Empty)
        {
            var byId = await _cashRegisterRepository.GetByIdAsync(clientId, cancellationToken);
            if (byId is not null)
            {
                return Result.Success(byId.Id);
            }
        }

        var existingOpenRegister = await _cashRegisterRepository.GetOpenCashRegisterByUserAsync(_tenantContext.UserId, cancellationToken);
        if (existingOpenRegister != null)
        {
            return Result.Failure<Guid>(Sales.Domain.ErrorCodes.CashRegister.AlreadyOpen);
        }

        var cashRegisterResult = request.CashRegisterId is Guid registerId && registerId != Guid.Empty
            ? CashRegister.Open(registerId, _tenantContext.UserId, request.OpeningBalance)
            : CashRegister.Open(_tenantContext.UserId, request.OpeningBalance);
        if (!cashRegisterResult.IsSuccess)
        {
            return Result.Failure<Guid>(cashRegisterResult.Error);
        }

        var cashRegister = cashRegisterResult.Value;
        _cashRegisterRepository.Add(cashRegister);

        return Result.Success(cashRegister.Id);
    }
}
