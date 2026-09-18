using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Services;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed vaccine store with outbox for dose registration.</summary>
public sealed class OfflineVaccineStore : IVaccineStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineVaccineStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<VaccineDoseListItemDto>>> GetDosesByPetAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var doses = await _dbContext.VaccineDoses.AsNoTracking()
            .Where(d => d.PetId == petId)
            .OrderByDescending(d => d.AppliedAt)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<VaccineDoseListItemDto>>(doses.Select(MapDose).ToList());
    }

    public async Task<Result<VaccinationCardDto>> GetCardAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == petId && !p.IsDeleted, cancellationToken);
        if (pet is null)
        {
            return Result.Failure<VaccinationCardDto>(ErrorCodes.Pet.NotFound);
        }

        var tutor = await _dbContext.Tutors.AsNoTracking().FirstOrDefaultAsync(t => t.Id == pet.TutorId, cancellationToken);
        var applied = await _dbContext.VaccineDoses.AsNoTracking().Where(d => d.PetId == petId).OrderByDescending(d => d.AppliedAt).ToListAsync(cancellationToken);
        var appliedIds = applied.Where(d => d.ProtocolDoseId.HasValue).Select(d => d.ProtocolDoseId!.Value).ToHashSet();
        var age = VaccineSchedule.GetAgeInDays(pet.BirthDate, DateOnly.FromDateTime(DateTime.UtcNow));

        var protocols = await _dbContext.VaccineProtocols.AsNoTracking()
            .Include(p => p.Doses)
            .Where(p => p.IsActive && p.Species == pet.Species)
            .ToListAsync(cancellationToken);

        var suggested = new List<SuggestedVaccineDoseListItemDto>();
        foreach (var protocol in protocols)
        {
            foreach (var dose in protocol.Doses.OrderBy(d => d.Sequence))
            {
                if (appliedIds.Contains(dose.Id))
                {
                    continue;
                }

                if (!VaccineSchedule.IsAgeEligible(age, dose.MinAgeInDays, dose.MaxAgeInDays))
                {
                    continue;
                }

                suggested.Add(new SuggestedVaccineDoseListItemDto
                {
                    ProtocolId = protocol.Id,
                    ProtocolName = protocol.Name,
                    ProtocolDoseId = dose.Id,
                    Label = dose.Label
                });
            }
        }

        return Result.Success(new VaccinationCardDto
        {
            PetId = pet.Id,
            PetName = pet.Name,
            Species = pet.Species.ToString(),
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            TutorName = tutor?.Name ?? string.Empty,
            AppliedDoses = applied.Select(MapDose).ToList(),
            SuggestedDoses = suggested
        });
    }

    public async Task<Result<Guid>> RegisterDoseAsync(RegisterVaccineDoseRequest request, CancellationToken cancellationToken = default)
    {
        var pet = await _dbContext.Pets.FirstOrDefaultAsync(p => p.Id == request.PetId && !p.IsDeleted, cancellationToken);
        if (pet is null)
        {
            return Result.Failure<Guid>(ErrorCodes.Pet.NotFound);
        }

        var id = request.Id == Guid.Empty ? Guid.NewGuid() : request.Id;
        var existing = await _dbContext.VaccineDoses.FindAsync([id], cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var name = request.Name;
        var nextDue = request.NextDueDate;
        Guid? protocolId = null;
        Guid? protocolDoseId = null;

        if (request.ProtocolDoseId is Guid pdId && pdId != Guid.Empty)
        {
            var doseLine = await _dbContext.VaccineProtocolDoses.AsNoTracking().FirstOrDefaultAsync(d => d.Id == pdId, cancellationToken);
            if (doseLine is null)
            {
                return Result.Failure<Guid>(new Error("VaccineDose.InvalidProtocolDose", "Protocol dose not found locally."));
            }

            var protocol = await _dbContext.VaccineProtocols.AsNoTracking().FirstOrDefaultAsync(p => p.Id == doseLine.VaccineProtocolId, cancellationToken);
            if (protocol is null || protocol.Species != pet.Species)
            {
                return Result.Failure<Guid>(new Error("VaccineDose.SpeciesMismatch", "Protocol species mismatch."));
            }

            protocolId = protocol.Id;
            protocolDoseId = doseLine.Id;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"{protocol.Name} — {doseLine.Label}";
            }

            nextDue ??= VaccineSchedule.ComputeNextDueDate(request.AppliedAt, doseLine.NextDoseIntervalInDays);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Guid>(new Error("VaccineDose.InvalidName", "Vaccine name is required."));
        }

        var created = VaccineDose.Create(id, request.PetId, name, request.BatchNumber, request.AppliedAt, nextDue, protocolId, protocolDoseId);
        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _dbContext.VaccineDoses.Add(created.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }

    public async Task<Result<IReadOnlyList<VaccineAlertListItemDto>>> GetAlertsAsync(VaccineAlertFilter filter, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var horizon = Math.Clamp(filter.HorizonDays, 1, 365);
        var doses = await _dbContext.VaccineDoses.AsNoTracking()
            .Where(d => d.NextDueDate != null)
            .ToListAsync(cancellationToken);

        var alerts = new List<VaccineAlertListItemDto>();
        foreach (var dose in doses.OrderBy(d => d.NextDueDate))
        {
            var kind = VaccineSchedule.ClassifyAlert(dose.NextDueDate, utcNow, horizon);
            if (kind == VaccineAlertKind.None)
            {
                continue;
            }

            var status = kind == VaccineAlertKind.Overdue ? "Overdue" : "Upcoming";
            if (!string.IsNullOrWhiteSpace(filter.Status) &&
                !string.Equals(filter.Status, status, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dose.PetId, cancellationToken);
            alerts.Add(new VaccineAlertListItemDto
            {
                VaccineDoseId = dose.Id,
                PetId = dose.PetId,
                PetName = pet?.Name ?? string.Empty,
                VaccineName = dose.Name,
                NextDueDate = dose.NextDueDate!.Value,
                Status = status
            });
        }

        return Result.Success<IReadOnlyList<VaccineAlertListItemDto>>(alerts);
    }

    public async Task<Result<int>> CountOverdueAlertsAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTimeOffset.UtcNow;
        var count = await _dbContext.VaccineDoses.AsNoTracking()
            .CountAsync(d => d.NextDueDate != null && d.NextDueDate < utcNow, cancellationToken);
        return Result.Success(count);
    }

    public async Task<Result<IReadOnlyList<VaccineProtocolListItemDto>>> ListProtocolsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.VaccineProtocols.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new VaccineProtocolListItemDto { Id = p.Id, Name = p.Name, Species = p.Species.ToString() })
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<VaccineProtocolListItemDto>>(list);
    }

    private static VaccineDoseListItemDto MapDose(VaccineDose dose) => new()
    {
        Id = dose.Id,
        Name = dose.Name,
        BatchNumber = dose.BatchNumber,
        AppliedAt = dose.AppliedAt,
        NextDueDate = dose.NextDueDate
    };
}
