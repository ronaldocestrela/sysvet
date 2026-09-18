using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Entities;
using MediatR;
using Veterinary.Application.Hospitalizations.Dtos;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.Services;
using VetErrors = Veterinary.Domain.ErrorCodes;

namespace Veterinary.Application.Hospitalizations.Commands;

public sealed class AdmitPetCommandHandler : IRequestHandler<AdmitPetCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IWardUnitRepository _wardUnitRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public AdmitPetCommandHandler(
        IHospitalizationRepository hospitalizationRepository,
        IWardUnitRepository wardUnitRepository,
        IPetRepository petRepository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _wardUnitRepository = wardUnitRepository;
        _petRepository = petRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(AdmitPetCommand request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet is null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.InvalidIdentifiers);
        }

        var bed = await _wardUnitRepository.FindBedAsync(request.BedId, cancellationToken);
        if (bed is null || !bed.Value.Bed.IsActive)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.InvalidBed);
        }

        if (await _hospitalizationRepository.GetActiveByPetAsync(request.PetId, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.PetAlreadyAdmitted);
        }

        if (await _hospitalizationRepository.GetActiveByBedAsync(request.BedId, cancellationToken) is not null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.BedOccupied);
        }

        var id = request.HospitalizationId == Guid.Empty ? Guid.NewGuid() : request.HospitalizationId;
        var admitResult = Hospitalization.Admit(
            id,
            request.PetId,
            request.VeterinarianId,
            request.BedId,
            request.Reason,
            DateTimeOffset.UtcNow);

        if (admitResult.IsFailure)
        {
            return Result.Failure<Guid>(admitResult.Error);
        }

        await _hospitalizationRepository.AddAsync(admitResult.Value, cancellationToken);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, id, "Hospitalization", "Admit", request.Reason, cancellationToken);
        return Result.Success(id);
    }
}

public sealed class DischargePetCommandHandler : IRequestHandler<DischargePetCommand, Result<bool>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public DischargePetCommandHandler(
        IHospitalizationRepository hospitalizationRepository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DischargePetCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<bool>(VetErrors.Hospitalization.NotFound);
        }

        var dischargeResult = hosp.Discharge(DateTimeOffset.UtcNow);
        if (dischargeResult.IsFailure)
        {
            return dischargeResult;
        }

        _hospitalizationRepository.Update(hosp);
        await _auditLogger.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, hosp.Id, "Hospitalization", "Discharge", string.Empty, cancellationToken);
        return Result.Success(true);
    }
}

public sealed class TransferHospitalizationBedCommandHandler : IRequestHandler<TransferHospitalizationBedCommand, Result>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IWardUnitRepository _wardUnitRepository;

    public TransferHospitalizationBedCommandHandler(
        IHospitalizationRepository hospitalizationRepository,
        IWardUnitRepository wardUnitRepository)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _wardUnitRepository = wardUnitRepository;
    }

    public async Task<Result> Handle(TransferHospitalizationBedCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _hospitalizationRepository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(VetErrors.Hospitalization.NotFound);
        }

        var bed = await _wardUnitRepository.FindBedAsync(request.NewBedId, cancellationToken);
        if (bed is null || !bed.Value.Bed.IsActive)
        {
            return Result.Failure(VetErrors.Hospitalization.InvalidBed);
        }

        var occupant = await _hospitalizationRepository.GetActiveByBedAsync(request.NewBedId, cancellationToken);
        if (occupant is not null && occupant.Id != hosp.Id)
        {
            return Result.Failure(VetErrors.Hospitalization.BedOccupied);
        }

        var transfer = hosp.TransferBed(request.NewBedId);
        if (transfer.IsFailure)
        {
            return transfer;
        }

        _hospitalizationRepository.Update(hosp);
        return Result.Success();
    }
}

public sealed class CreateMedicationOrderCommandHandler : IRequestHandler<CreateMedicationOrderCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _repository;

    public CreateMedicationOrderCommandHandler(IHospitalizationRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(CreateMedicationOrderCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.NotFound);
        }

        var orderId = request.OrderId == Guid.Empty ? Guid.NewGuid() : request.OrderId;
        var add = hosp.AddMedicationOrder(
            orderId,
            request.MedicationName,
            request.Dose,
            request.Route,
            request.DailyTimes,
            request.StartsOn,
            request.EndsOn);

        if (add.IsFailure)
        {
            return add;
        }

        _repository.Update(hosp);
        return Result.Success(add.Value);
    }
}

public sealed class AdministerMedicationCommandHandler : IRequestHandler<AdministerMedicationCommand, Result>
{
    private readonly IHospitalizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public AdministerMedicationCommandHandler(IHospitalizationRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(AdministerMedicationCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(VetErrors.Hospitalization.NotFound);
        }

        var actorId = _tenantContext.UserId;
        var result = hosp.AdministerMedication(request.AdministrationId, actorId, request.Notes, DateTimeOffset.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(hosp);
        return Result.Success();
    }
}

public sealed class SkipMedicationCommandHandler : IRequestHandler<SkipMedicationCommand, Result>
{
    private readonly IHospitalizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public SkipMedicationCommandHandler(IHospitalizationRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SkipMedicationCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(VetErrors.Hospitalization.NotFound);
        }

        var result = hosp.SkipMedication(request.AdministrationId, _tenantContext.UserId, request.Notes, DateTimeOffset.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(hosp);
        return Result.Success();
    }
}

public sealed class AddHospitalizationProgressNoteCommandHandler : IRequestHandler<AddHospitalizationProgressNoteCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public AddHospitalizationProgressNoteCommandHandler(IHospitalizationRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(AddHospitalizationProgressNoteCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.NotFound);
        }

        var noteId = request.NoteId == Guid.Empty ? Guid.NewGuid() : request.NoteId;
        var add = hosp.AddProgressNote(noteId, _tenantContext.UserId, request.Text, DateTimeOffset.UtcNow);
        if (add.IsFailure)
        {
            return Result.Failure<Guid>(add.Error);
        }

        _repository.Update(hosp);
        return Result.Success(noteId);
    }
}

public sealed class AddHospitalProcedureCommandHandler : IRequestHandler<AddHospitalProcedureCommand, Result<Guid>>
{
    private readonly IHospitalizationRepository _repository;

    public AddHospitalProcedureCommandHandler(IHospitalizationRepository repository) => _repository = repository;

    public async Task<Result<Guid>> Handle(AddHospitalProcedureCommand request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<Guid>(VetErrors.Hospitalization.NotFound);
        }

        var procedureId = request.ProcedureId == Guid.Empty ? Guid.NewGuid() : request.ProcedureId;
        var add = hosp.AddProcedure(procedureId, request.Name, request.VeterinarianId, request.PerformedAt, request.Notes);
        if (add.IsFailure)
        {
            return Result.Failure<Guid>(add.Error);
        }

        _repository.Update(hosp);
        return Result.Success(procedureId);
    }
}

public sealed class GetHospitalizationByIdQueryHandler : IRequestHandler<GetHospitalizationByIdQuery, Result<HospitalizationDetailDto>>
{
    private readonly IHospitalizationRepository _repository;

    public GetHospitalizationByIdQueryHandler(IHospitalizationRepository repository) => _repository = repository;

    public async Task<Result<HospitalizationDetailDto>> Handle(GetHospitalizationByIdQuery request, CancellationToken cancellationToken)
    {
        var hosp = await _repository.GetByIdAsync(request.HospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<HospitalizationDetailDto>(VetErrors.Hospitalization.NotFound);
        }

        return Result.Success(new HospitalizationDetailDto
        {
            Id = hosp.Id,
            PetId = hosp.PetId,
            VeterinarianId = hosp.VeterinarianId,
            BedId = hosp.BedId,
            Reason = hosp.Reason,
            AdmittedAt = hosp.AdmittedAt,
            DischargedAt = hosp.DischargedAt,
            Status = hosp.Status.ToString()
        });
    }
}

public sealed class ListActiveHospitalizationsQueryHandler : IRequestHandler<ListActiveHospitalizationsQuery, Result<IReadOnlyList<HospitalizationListItemDto>>>
{
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IPetRepository _petRepository;

    public ListActiveHospitalizationsQueryHandler(IHospitalizationRepository hospitalizationRepository, IPetRepository petRepository)
    {
        _hospitalizationRepository = hospitalizationRepository;
        _petRepository = petRepository;
    }

    public async Task<Result<IReadOnlyList<HospitalizationListItemDto>>> Handle(ListActiveHospitalizationsQuery request, CancellationToken cancellationToken)
    {
        var active = await _hospitalizationRepository.GetActiveAsync(cancellationToken);
        var dtos = new List<HospitalizationListItemDto>();
        foreach (var hosp in active)
        {
            var pet = await _petRepository.GetByIdAsync(hosp.PetId, cancellationToken);
            dtos.Add(new HospitalizationListItemDto
            {
                Id = hosp.Id,
                PetId = hosp.PetId,
                PetName = pet?.Name ?? string.Empty,
                BedId = hosp.BedId,
                Reason = hosp.Reason,
                AdmittedAt = hosp.AdmittedAt,
                Status = hosp.Status.ToString()
            });
        }

        return Result.Success<IReadOnlyList<HospitalizationListItemDto>>(dtos);
    }
}

public sealed class GetExecutionMapQueryHandler : IRequestHandler<GetExecutionMapQuery, Result<ExecutionMapDto>>
{
    private readonly IWardUnitRepository _wardUnitRepository;
    private readonly IHospitalizationRepository _hospitalizationRepository;
    private readonly IPetRepository _petRepository;

    public GetExecutionMapQueryHandler(
        IWardUnitRepository wardUnitRepository,
        IHospitalizationRepository hospitalizationRepository,
        IPetRepository petRepository)
    {
        _wardUnitRepository = wardUnitRepository;
        _hospitalizationRepository = hospitalizationRepository;
        _petRepository = petRepository;
    }

    public async Task<Result<ExecutionMapDto>> Handle(GetExecutionMapQuery request, CancellationToken cancellationToken)
    {
        var utcDay = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var wards = await _wardUnitRepository.ListAsync(activeOnly: true, cancellationToken);
        var active = await _hospitalizationRepository.GetActiveAsync(cancellationToken);
        var byBed = active.ToDictionary(h => h.BedId);

        var orderLookup = active
            .SelectMany(h => h.MedicationOrders.Select(o => (Order: o, HospId: h.Id)))
            .ToDictionary(x => x.Order.Id, x => x.Order);

        var wardDtos = new List<ExecutionMapWardDto>();
        foreach (var ward in wards)
        {
            var bedDtos = new List<ExecutionMapBedDto>();
            foreach (var bed in ward.Beds.Where(b => b.IsActive).OrderBy(b => b.SortOrder))
            {
                ExecutionMapOccupancyDto? occupancy = null;
                if (byBed.TryGetValue(bed.Id, out var hosp))
                {
                    var pet = await _petRepository.GetByIdAsync(hosp.PetId, cancellationToken);
                    var admins = hosp.Administrations
                        .Where(a => MedicationSchedule.IsOnUtcDay(a.ScheduledAt, utcDay))
                        .OrderBy(a => a.ScheduledAt)
                        .Select(a =>
                        {
                            orderLookup.TryGetValue(a.MedicationOrderId, out var order);
                            return new ExecutionMapAdministrationDto
                            {
                                Id = a.Id,
                                MedicationOrderId = a.MedicationOrderId,
                                MedicationName = order?.MedicationName ?? string.Empty,
                                Dose = order?.Dose ?? string.Empty,
                                Route = order?.Route ?? string.Empty,
                                ScheduledAt = a.ScheduledAt,
                                Status = a.Status.ToString()
                            };
                        }).ToList();

                    occupancy = new ExecutionMapOccupancyDto
                    {
                        HospitalizationId = hosp.Id,
                        PetId = hosp.PetId,
                        PetName = pet?.Name ?? string.Empty,
                        Reason = hosp.Reason,
                        Administrations = admins
                    };
                }

                bedDtos.Add(new ExecutionMapBedDto
                {
                    BedId = bed.Id,
                    BedCode = bed.Code,
                    Occupancy = occupancy
                });
            }

            wardDtos.Add(new ExecutionMapWardDto
            {
                WardUnitId = ward.Id,
                WardName = ward.Name,
                Beds = bedDtos
            });
        }

        return Result.Success(new ExecutionMapDto { Date = utcDay, Wards = wardDtos });
    }
}
