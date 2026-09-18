using Core.Domain;
using Core.Domain.Auditing;
using Core.Domain.Entities;
using MediatR;
using Veterinary.Application.Vaccines.Dtos;
using Veterinary.Domain.Entities;
using VetErrors = Veterinary.Domain.ErrorCodes;
using Veterinary.Domain.Repositories;
using Veterinary.Domain.Services;

namespace Veterinary.Application.Vaccines.Commands;

public sealed class CreateVaccineProtocolCommandHandler : IRequestHandler<CreateVaccineProtocolCommand, Result<Guid>>
{
    private readonly IVaccineProtocolRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public CreateVaccineProtocolCommandHandler(
        IVaccineProtocolRepository repository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateVaccineProtocolCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var created = VaccineProtocol.Create(id, request.Name, request.Species);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        var protocol = created.Value;
        var doses = request.Doses.Select(d => (
            d.DoseId ?? Guid.Empty,
            d.Label,
            d.MinAgeInDays,
            d.MaxAgeInDays,
            d.IntervalFromPreviousInDays,
            d.NextDoseIntervalInDays)).ToList();

        var replace = protocol.ReplaceDoses(doses);
        if (replace.IsFailure)
        {
            return Result.Failure<Guid>(replace.Error);
        }

        await _repository.AddAsync(protocol, cancellationToken);
        await VaccineAuditHelper.LogAsync(_auditLogger, _tenantContext, protocol.Id, "VaccineProtocol", "Create", $"species={protocol.Species}", cancellationToken);
        return Result.Success(protocol.Id);
    }
}

public sealed class UpdateVaccineProtocolCommandHandler : IRequestHandler<UpdateVaccineProtocolCommand, Result>
{
    private readonly IVaccineProtocolRepository _repository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public UpdateVaccineProtocolCommandHandler(
        IVaccineProtocolRepository repository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateVaccineProtocolCommand request, CancellationToken cancellationToken)
    {
        var protocol = await _repository.GetByIdAsync(request.ProtocolId, cancellationToken);
        if (protocol is null)
        {
            return Result.Failure(VetErrors.VaccineProtocol.NotFound);
        }

        var update = protocol.UpdateDetails(request.Name, request.Species);
        if (update.IsFailure)
        {
            return update;
        }

        var doses = request.Doses.Select(d => (
            d.DoseId ?? Guid.Empty,
            d.Label,
            d.MinAgeInDays,
            d.MaxAgeInDays,
            d.IntervalFromPreviousInDays,
            d.NextDoseIntervalInDays)).ToList();

        var replace = protocol.ReplaceDoses(doses);
        if (replace.IsFailure)
        {
            return replace;
        }

        _repository.Update(protocol);
        await VaccineAuditHelper.LogAsync(_auditLogger, _tenantContext, protocol.Id, "VaccineProtocol", "Update", "doses replaced", cancellationToken);
        return Result.Success();
    }
}

public sealed class DeactivateVaccineProtocolCommandHandler : IRequestHandler<DeactivateVaccineProtocolCommand, Result>
{
    private readonly IVaccineProtocolRepository _repository;

    public DeactivateVaccineProtocolCommandHandler(IVaccineProtocolRepository repository) => _repository = repository;

    public async Task<Result> Handle(DeactivateVaccineProtocolCommand request, CancellationToken cancellationToken)
    {
        var protocol = await _repository.GetByIdAsync(request.ProtocolId, cancellationToken);
        if (protocol is null)
        {
            return Result.Failure(VetErrors.VaccineProtocol.NotFound);
        }

        var result = protocol.Deactivate();
        if (result.IsFailure)
        {
            return result;
        }

        _repository.Update(protocol);
        return Result.Success();
    }
}

public sealed class RegisterVaccineDoseCommandHandler : IRequestHandler<RegisterVaccineDoseCommand, Result<Guid>>
{
    private readonly IVaccineDoseRepository _vaccineDoseRepository;
    private readonly IVaccineProtocolRepository _protocolRepository;
    private readonly IPetRepository _petRepository;
    private readonly IAuditLogger _auditLogger;
    private readonly ITenantContext _tenantContext;

    public RegisterVaccineDoseCommandHandler(
        IVaccineDoseRepository vaccineDoseRepository,
        IVaccineProtocolRepository protocolRepository,
        IPetRepository petRepository,
        IAuditLogger auditLogger,
        ITenantContext tenantContext)
    {
        _vaccineDoseRepository = vaccineDoseRepository;
        _protocolRepository = protocolRepository;
        _petRepository = petRepository;
        _auditLogger = auditLogger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(RegisterVaccineDoseCommand request, CancellationToken cancellationToken)
    {
        var doseId = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
        if (request.Id != Guid.Empty)
        {
            var existing = await _vaccineDoseRepository.GetByIdAsync(doseId, cancellationToken);
            if (existing is not null)
            {
                return Result.Success(existing.Id);
            }
        }

        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet is null || pet.IsDeleted)
        {
            return Result.Failure<Guid>(Core.Domain.ErrorCodes.Pet.NotFound);
        }

        var name = request.Name;
        var nextDue = request.NextDueDate;
        Guid? protocolId = null;
        Guid? protocolDoseId = null;

        if (request.ProtocolDoseId is Guid pdId && pdId != Guid.Empty)
        {
            var match = await _protocolRepository.FindDoseAsync(pdId, cancellationToken);
            if (match is null)
            {
                return Result.Failure<Guid>(VetErrors.VaccineDose.InvalidProtocolDose);
            }

            if (match.Value.Protocol.Species != pet.Species)
            {
                return Result.Failure<Guid>(VetErrors.VaccineDose.SpeciesMismatch);
            }

            protocolId = match.Value.Protocol.Id;
            protocolDoseId = match.Value.Dose.Id;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"{match.Value.Protocol.Name} — {match.Value.Dose.Label}";
            }

            nextDue ??= VaccineSchedule.ComputeNextDueDate(request.AppliedAt, match.Value.Dose.NextDoseIntervalInDays);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Guid>(VetErrors.VaccineDose.InvalidName);
        }

        var vaccineResult = VaccineDose.Create(
            doseId,
            request.PetId,
            name,
            request.BatchNumber,
            request.AppliedAt,
            nextDue,
            protocolId,
            protocolDoseId);

        if (vaccineResult.IsFailure)
        {
            return Result.Failure<Guid>(vaccineResult.Error);
        }

        await _vaccineDoseRepository.AddAsync(vaccineResult.Value, cancellationToken);
        await VaccineAuditHelper.LogAsync(
            _auditLogger,
            _tenantContext,
            vaccineResult.Value.Id,
            "VaccineDose",
            "Register",
            $"pet={request.PetId:N}; nextDue={nextDue:O}",
            cancellationToken);

        return Result.Success(vaccineResult.Value.Id);
    }
}

public sealed class ListVaccineProtocolsQueryHandler : IRequestHandler<ListVaccineProtocolsQuery, Result<IReadOnlyList<VaccineProtocolDto>>>
{
    private readonly IVaccineProtocolRepository _repository;

    public ListVaccineProtocolsQueryHandler(IVaccineProtocolRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<VaccineProtocolDto>>> Handle(ListVaccineProtocolsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repository.ListAsync(request.Species, request.ActiveOnly, cancellationToken);
        return Result.Success<IReadOnlyList<VaccineProtocolDto>>(list.Select(VaccineMapper.ToDto).ToList());
    }
}

public sealed class ListVaccineDosesByPetQueryHandler : IRequestHandler<ListVaccineDosesByPetQuery, Result<IReadOnlyList<VaccineDoseDto>>>
{
    private readonly IVaccineDoseRepository _repository;

    public ListVaccineDosesByPetQueryHandler(IVaccineDoseRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<VaccineDoseDto>>> Handle(ListVaccineDosesByPetQuery request, CancellationToken cancellationToken)
    {
        var doses = await _repository.GetByPetIdAsync(request.PetId, cancellationToken);
        return Result.Success<IReadOnlyList<VaccineDoseDto>>(doses.Select(VaccineMapper.ToDto).ToList());
    }
}

public sealed class GetVaccinationCardQueryHandler : IRequestHandler<GetVaccinationCardQuery, Result<VaccinationCardDto>>
{
    private readonly IVaccineDoseRepository _doseRepository;
    private readonly IVaccineProtocolRepository _protocolRepository;
    private readonly IPetRepository _petRepository;
    private readonly ITutorRepository _tutorRepository;

    public GetVaccinationCardQueryHandler(
        IVaccineDoseRepository doseRepository,
        IVaccineProtocolRepository protocolRepository,
        IPetRepository petRepository,
        ITutorRepository tutorRepository)
    {
        _doseRepository = doseRepository;
        _protocolRepository = protocolRepository;
        _petRepository = petRepository;
        _tutorRepository = tutorRepository;
    }

    public async Task<Result<VaccinationCardDto>> Handle(GetVaccinationCardQuery request, CancellationToken cancellationToken)
    {
        var pet = await _petRepository.GetByIdAsync(request.PetId, cancellationToken);
        if (pet is null || pet.IsDeleted)
        {
            return Result.Failure<VaccinationCardDto>(Core.Domain.ErrorCodes.Pet.NotFound);
        }

        var tutor = await _tutorRepository.GetByIdAsync(pet.TutorId, cancellationToken);
        var applied = await _doseRepository.GetByPetIdAsync(request.PetId, cancellationToken);
        var appliedDoseIds = applied.Where(d => d.ProtocolDoseId.HasValue).Select(d => d.ProtocolDoseId!.Value).ToHashSet();

        var referenceDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var ageInDays = VaccineSchedule.GetAgeInDays(pet.BirthDate, referenceDate);

        var protocols = await _protocolRepository.ListAsync(pet.Species, activeOnly: true, cancellationToken);
        var suggested = new List<SuggestedVaccineDoseDto>();
        foreach (var protocol in protocols)
        {
            foreach (var dose in protocol.Doses.OrderBy(d => d.Sequence))
            {
                if (appliedDoseIds.Contains(dose.Id))
                {
                    continue;
                }

                if (!VaccineSchedule.IsAgeEligible(ageInDays, dose.MinAgeInDays, dose.MaxAgeInDays))
                {
                    continue;
                }

                suggested.Add(new SuggestedVaccineDoseDto
                {
                    ProtocolId = protocol.Id,
                    ProtocolName = protocol.Name,
                    ProtocolDoseId = dose.Id,
                    Label = dose.Label,
                    Sequence = dose.Sequence
                });
            }
        }

        var card = new VaccinationCardDto
        {
            PetId = pet.Id,
            PetName = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            TutorName = tutor?.Name ?? string.Empty,
            AppliedDoses = applied.Select(VaccineMapper.ToDto).ToList(),
            SuggestedDoses = suggested
        };

        return Result.Success(card);
    }
}

public sealed class ListVaccineAlertsQueryHandler : IRequestHandler<ListVaccineAlertsQuery, Result<IReadOnlyList<VaccineAlertDto>>>
{
    private readonly IVaccineDoseRepository _doseRepository;
    private readonly IPetRepository _petRepository;

    public ListVaccineAlertsQueryHandler(IVaccineDoseRepository doseRepository, IPetRepository petRepository)
    {
        _doseRepository = doseRepository;
        _petRepository = petRepository;
    }

    public async Task<Result<IReadOnlyList<VaccineAlertDto>>> Handle(ListVaccineAlertsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var horizon = Math.Clamp(request.HorizonDays, 1, 365);
        var take = Math.Clamp(request.PageSize, 1, 200);

        var candidates = new List<VaccineDose>();
        if (request.Status is null or VaccineAlertStatusDto.Overdue)
        {
            candidates.AddRange(await _doseRepository.GetOverdueAsync(utcNow, take, cancellationToken));
        }

        if (request.Status is null or VaccineAlertStatusDto.Upcoming)
        {
            var until = utcNow.AddDays(horizon);
            candidates.AddRange(await _doseRepository.GetDueAsync(utcNow, until, take, cancellationToken));
        }

        var alerts = new List<VaccineAlertDto>();
        foreach (var dose in candidates.DistinctBy(d => d.Id).OrderBy(d => d.NextDueDate))
        {
            if (dose.NextDueDate is null)
            {
                continue;
            }

            var kind = VaccineSchedule.ClassifyAlert(dose.NextDueDate, utcNow, horizon);
            if (kind == VaccineAlertKind.None)
            {
                continue;
            }

            if (request.Status == VaccineAlertStatusDto.Overdue && kind != VaccineAlertKind.Overdue)
            {
                continue;
            }

            if (request.Status == VaccineAlertStatusDto.Upcoming && kind != VaccineAlertKind.Upcoming)
            {
                continue;
            }

            var pet = await _petRepository.GetByIdAsync(dose.PetId, cancellationToken);
            alerts.Add(new VaccineAlertDto
            {
                VaccineDoseId = dose.Id,
                PetId = dose.PetId,
                PetName = pet?.Name ?? string.Empty,
                VaccineName = dose.Name,
                NextDueDate = dose.NextDueDate.Value,
                Status = kind == VaccineAlertKind.Overdue ? VaccineAlertStatusDto.Overdue : VaccineAlertStatusDto.Upcoming
            });
        }

        var skip = (Math.Max(request.Page, 1) - 1) * take;
        return Result.Success<IReadOnlyList<VaccineAlertDto>>(alerts.Skip(skip).Take(take).ToList());
    }
}
