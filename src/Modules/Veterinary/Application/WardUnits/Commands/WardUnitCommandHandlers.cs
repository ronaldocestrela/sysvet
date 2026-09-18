using Core.Domain;
using Core.Domain.Auditing;
using MediatR;
using Veterinary.Application.WardUnits.Dtos;
using Veterinary.Domain.Entities;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Repositories;

namespace Veterinary.Application.WardUnits.Commands;

public sealed class CreateWardUnitCommandHandler : IRequestHandler<CreateWardUnitCommand, Result<Guid>>
{
    private readonly IWardUnitRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public CreateWardUnitCommandHandler(IWardUnitRepository repository, IAuditLogger auditLogger, ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateWardUnitCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var created = WardUnit.Create(id, request.Name);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        var unit = created.Value;
        if (request.Beds.Count > 0)
        {
            var lines = request.Beds.Select(b => (b.BedId ?? Guid.Empty, b.Code, b.SortOrder, b.IsActive));
            var replace = unit.ReplaceBeds(lines);
            if (replace.IsFailure)
            {
                return Result.Failure<Guid>(replace.Error);
            }
        }

        await _repository.AddAsync(unit, cancellationToken);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, unit.Id, "WardUnit", "Create", unit.Name, cancellationToken);
        return Result.Success(unit.Id);
    }
}

public sealed class UpdateWardUnitCommandHandler : IRequestHandler<UpdateWardUnitCommand, Result>
{
    private readonly IWardUnitRepository _repository;

    public UpdateWardUnitCommandHandler(IWardUnitRepository repository) => _repository = repository;

    public async Task<Result> Handle(UpdateWardUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.WardUnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Failure(VetErrors.WardUnit.NotFound);
        }

        var update = unit.UpdateName(request.Name);
        if (update.IsFailure)
        {
            return update;
        }

        _repository.Update(unit);
        return Result.Success();
    }
}

public sealed class ReplaceWardUnitBedsCommandHandler : IRequestHandler<ReplaceWardUnitBedsCommand, Result>
{
    private readonly IWardUnitRepository _repository;

    public ReplaceWardUnitBedsCommandHandler(IWardUnitRepository repository) => _repository = repository;

    public async Task<Result> Handle(ReplaceWardUnitBedsCommand request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.WardUnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Failure(VetErrors.WardUnit.NotFound);
        }

        var lines = request.Beds.Select(b => (b.BedId ?? Guid.Empty, b.Code, b.SortOrder, b.IsActive));
        var replace = unit.ReplaceBeds(lines);
        if (replace.IsFailure)
        {
            return replace;
        }

        _repository.Update(unit);
        return Result.Success();
    }
}

public sealed class DeactivateWardUnitCommandHandler : IRequestHandler<DeactivateWardUnitCommand, Result>
{
    private readonly IWardUnitRepository _repository;

    public DeactivateWardUnitCommandHandler(IWardUnitRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeactivateWardUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.WardUnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Failure(VetErrors.WardUnit.NotFound);
        }

        var deactivate = unit.Deactivate();
        if (deactivate.IsFailure)
        {
            return deactivate;
        }

        _repository.Update(unit);
        return Result.Success();
    }
}

public sealed class ListWardUnitsQueryHandler : IRequestHandler<ListWardUnitsQuery, Result<IReadOnlyList<WardUnitListItemDto>>>
{
    private readonly IWardUnitRepository _repository;

    public ListWardUnitsQueryHandler(IWardUnitRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<WardUnitListItemDto>>> Handle(ListWardUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await _repository.ListAsync(request.ActiveOnly, cancellationToken);
        var dtos = units.Select(u => new WardUnitListItemDto
        {
            Id = u.Id,
            Name = u.Name,
            IsActive = u.IsActive,
            BedCount = u.Beds.Count
        }).ToList();

        return Result.Success<IReadOnlyList<WardUnitListItemDto>>(dtos);
    }
}

public sealed class GetWardUnitByIdQueryHandler : IRequestHandler<GetWardUnitByIdQuery, Result<WardUnitDetailDto>>
{
    private readonly IWardUnitRepository _repository;

    public GetWardUnitByIdQueryHandler(IWardUnitRepository repository) => _repository = repository;

    public async Task<Result<WardUnitDetailDto>> Handle(GetWardUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.WardUnitId, cancellationToken);
        if (unit is null)
        {
            return Result.Failure<WardUnitDetailDto>(VetErrors.WardUnit.NotFound);
        }

        return Result.Success(new WardUnitDetailDto
        {
            Id = unit.Id,
            Name = unit.Name,
            IsActive = unit.IsActive,
            Beds = unit.Beds.OrderBy(b => b.SortOrder).Select(b => new BedDto
            {
                Id = b.Id,
                Code = b.Code,
                SortOrder = b.SortOrder,
                IsActive = b.IsActive
            }).ToList()
        });
    }
}
