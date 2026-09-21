using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Application.GroomingServices;

using ErrorCodes = Petshop.Domain.ErrorCodes;

public sealed class UpsertGroomingServiceCommandHandler : IRequestHandler<UpsertGroomingServiceCommand, Result<Guid>>
{
    private readonly IGroomingServiceRepository _repository;

    public UpsertGroomingServiceCommandHandler(IGroomingServiceRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(UpsertGroomingServiceCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is null)
        {
            var create = GroomingService.Create(
                request.Id,
                request.Name,
                request.ServiceType,
                request.DurationInMinutes,
                request.PrepaidServiceCode);

            if (create.IsFailure)
            {
                return Result.Failure<Guid>(create.Error);
            }

            var supplies = create.Value.SetDefaultSupplies(request.DefaultSupplies.Select(l => (l.ProductId, l.Quantity)));
            if (supplies.IsFailure)
            {
                return Result.Failure<Guid>(supplies.Error);
            }

            await _repository.AddAsync(create.Value, cancellationToken);
            return Result.Success(create.Value.Id);
        }

        var update = existing.Update(request.Name, request.ServiceType, request.DurationInMinutes, request.PrepaidServiceCode, request.IsActive);
        if (update.IsFailure)
        {
            return Result.Failure<Guid>(update.Error);
        }

        var setSupplies = existing.SetDefaultSupplies(request.DefaultSupplies.Select(l => (l.ProductId, l.Quantity)));
        if (setSupplies.IsFailure)
        {
            return Result.Failure<Guid>(setSupplies.Error);
        }

        _repository.Update(existing);
        return Result.Success(existing.Id);
    }
}

public sealed class ListGroomingServicesQueryHandler : IRequestHandler<ListGroomingServicesQuery, Result<IReadOnlyList<GroomingServiceDto>>>
{
    private readonly IGroomingServiceRepository _repository;

    public ListGroomingServicesQueryHandler(IGroomingServiceRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<GroomingServiceDto>>> Handle(ListGroomingServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await _repository.ListAllAsync(cancellationToken);
        var dtos = services.Select(s => new GroomingServiceDto(
            s.Id,
            s.Name,
            s.ServiceType,
            s.DurationInMinutes,
            s.PrepaidServiceCode,
            s.IsActive,
            s.DefaultSupplies.Select(l => new GroomingServiceSupplyLineDto(l.ProductId, l.Quantity)).ToList())).ToList();

        return Result.Success<IReadOnlyList<GroomingServiceDto>>(dtos);
    }
}
