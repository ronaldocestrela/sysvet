using Core.Application.Messaging;
using Core.Domain;
using MediatR;
using Finance.Application.CostCenters.Dtos;
using Finance.Domain.Entities;
using Finance.Domain.Repositories;

namespace Finance.Application.CostCenters;

public sealed class ListCostCentersQueryHandler : IRequestHandler<ListCostCentersQuery, Result<IReadOnlyList<CostCenterDto>>>
{
    private readonly ICostCenterRepository _repository;

    public ListCostCentersQueryHandler(ICostCenterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CostCenterDto>>> Handle(ListCostCentersQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<CostCenterDto>>(items.Select(c => new CostCenterDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            IsActive = c.IsActive
        }).ToList());
    }
}

public sealed class UpsertCostCenterCommandHandler : IRequestHandler<UpsertCostCenterCommand, Result<Guid>>
{
    private readonly ICostCenterRepository _repository;

    public UpsertCostCenterCommandHandler(ICostCenterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(UpsertCostCenterCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (existing is not null)
        {
            var update = existing.Update(request.Name, request.IsActive);
            if (update.IsFailure)
            {
                return Result.Failure<Guid>(update.Error);
            }

            _repository.Update(existing);
            return Result.Success(existing.Id);
        }

        var created = CostCenter.Create(request.Code, request.Name, request.Id);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _repository.Add(created.Value);
        return Result.Success(created.Value.Id);
    }
}
