using Core.Application.IntegrationEvents;
using Core.Domain;
using MediatR;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Application.Prepaid;

public sealed class ConsumePrepaidPackageUseCommandHandler : IRequestHandler<ConsumePrepaidPackageUseCommand, Result<bool>>
{
    private readonly IPrepaidBalanceRepository _balanceRepository;

    public ConsumePrepaidPackageUseCommandHandler(IPrepaidBalanceRepository balanceRepository)
        => _balanceRepository = balanceRepository;

    public async Task<Result<bool>> Handle(ConsumePrepaidPackageUseCommand request, CancellationToken cancellationToken)
    {
        var balance = await _balanceRepository.GetByPetAndServiceAsync(request.PetId, request.ServiceCode, cancellationToken);
        if (balance is null)
        {
            return Result.Failure<bool>(Sales.Domain.ErrorCodes.Package.NotFound);
        }

        var consume = balance.Consume(request.ServiceCode, request.PetId);
        if (consume.IsFailure)
        {
            return Result.Failure<bool>(consume.Error);
        }

        return Result.Success(true);
    }
}

/// <summary>Cross-module entry point for prepaid consumption (ADR-029).</summary>
public sealed class ConsumePrepaidServicePackageRequestHandler : IRequestHandler<ConsumePrepaidServicePackageRequest, Result>
{
    private readonly IMediator _mediator;

    public ConsumePrepaidServicePackageRequestHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result> Handle(ConsumePrepaidServicePackageRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ServiceCode>(request.ServiceCode, true, out var serviceCode))
        {
            return Result.Failure(Sales.Domain.ErrorCodes.Package.ServiceMismatch);
        }

        var command = new ConsumePrepaidPackageUseCommand
        {
            UsageId = request.UsageId,
            PetId = request.PetId,
            ServiceCode = serviceCode,
            AttendanceRef = request.AttendanceRef,
            IdempotencyKey = request.UsageId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error);
    }
}

public sealed class ListPrepaidBalancesQueryHandler : IRequestHandler<ListPrepaidBalancesQuery, Result<IReadOnlyList<PrepaidBalanceDto>>>
{
    private readonly IPrepaidBalanceRepository _repository;

    public ListPrepaidBalancesQueryHandler(IPrepaidBalanceRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<PrepaidBalanceDto>>> Handle(ListPrepaidBalancesQuery request, CancellationToken cancellationToken)
    {
        var balances = await _repository.ListAsync(request.TutorId, request.PetId, request.ServiceCode, cancellationToken);
        var dtos = balances
            .Select(b => new PrepaidBalanceDto(b.Id, b.TutorId, b.PetId, b.ServiceCode, b.RemainingUses, b.PurchasedUses))
            .ToList();
        return Result.Success<IReadOnlyList<PrepaidBalanceDto>>(dtos);
    }
}
