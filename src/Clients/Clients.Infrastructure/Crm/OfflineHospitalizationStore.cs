using Clients.Infrastructure.Sync;
using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;
using Veterinary.Domain.Services;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed hospitalization store with sync outbox.</summary>
public sealed class OfflineHospitalizationStore : IHospitalizationStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineHospitalizationStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<ExecutionMapViewModel>> GetExecutionMapAsync(DateOnly? date = null, CancellationToken cancellationToken = default)
    {
        var utcDay = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var wards = await _dbContext.WardUnits.AsNoTracking().Include(u => u.Beds).Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync(cancellationToken);
        var active = await _dbContext.Hospitalizations.AsNoTracking()
            .Include(h => h.MedicationOrders)
            .Include(h => h.Administrations)
            .Where(h => h.Status == HospitalizationStatus.Admitted)
            .ToListAsync(cancellationToken);
        var byBed = active.ToDictionary(h => h.BedId);
        var orderLookup = active.SelectMany(h => h.MedicationOrders.Select(o => (Order: o, HospId: h.Id))).ToDictionary(x => x.Order.Id, x => x.Order);

        var wardModels = new List<ExecutionMapWardViewModel>();
        foreach (var ward in wards)
        {
            var beds = new List<ExecutionMapBedViewModel>();
            foreach (var bed in ward.Beds.Where(b => b.IsActive).OrderBy(b => b.SortOrder))
            {
                string petName = string.Empty;
                Guid? hospId = null;
                var admins = new List<ExecutionMapAdministrationViewModel>();
                if (byBed.TryGetValue(bed.Id, out var hosp))
                {
                    hospId = hosp.Id;
                    var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == hosp.PetId, cancellationToken);
                    petName = pet?.Name ?? string.Empty;
                    admins = hosp.Administrations
                        .Where(a => MedicationSchedule.IsOnUtcDay(a.ScheduledAt, utcDay))
                        .OrderBy(a => a.ScheduledAt)
                        .Select(a =>
                        {
                            orderLookup.TryGetValue(a.MedicationOrderId, out var order);
                            return new ExecutionMapAdministrationViewModel
                            {
                                Id = a.Id,
                                MedicationName = order?.MedicationName ?? string.Empty,
                                Dose = order?.Dose ?? string.Empty,
                                ScheduledAt = a.ScheduledAt,
                                Status = a.Status.ToString()
                            };
                        }).ToList();
                }

                beds.Add(new ExecutionMapBedViewModel
                {
                    BedId = bed.Id,
                    BedCode = bed.Code,
                    HospitalizationId = hospId,
                    PetName = petName,
                    Administrations = admins
                });
            }

            wardModels.Add(new ExecutionMapWardViewModel { WardUnitId = ward.Id, WardName = ward.Name, Beds = beds });
        }

        return Result.Success(new ExecutionMapViewModel { Date = utcDay, Wards = wardModels });
    }

    public async Task<Result<IReadOnlyList<HospitalizationListViewModel>>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        var list = await _dbContext.Hospitalizations.AsNoTracking()
            .Where(h => h.Status == HospitalizationStatus.Admitted)
            .OrderBy(h => h.AdmittedAt)
            .ToListAsync(cancellationToken);

        var result = new List<HospitalizationListViewModel>();
        foreach (var h in list)
        {
            var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == h.PetId, cancellationToken);
            result.Add(new HospitalizationListViewModel
            {
                Id = h.Id,
                PetId = h.PetId,
                PetName = pet?.Name ?? string.Empty,
                BedId = h.BedId,
                Reason = h.Reason,
                AdmittedAt = h.AdmittedAt
            });
        }

        return Result.Success<IReadOnlyList<HospitalizationListViewModel>>(result);
    }

    public async Task<Result<Guid>> AdmitAsync(Guid petId, Guid veterinarianId, Guid bedId, string reason, CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Hospitalizations.AnyAsync(h => h.PetId == petId && h.Status == HospitalizationStatus.Admitted, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Hospitalization.PetAlreadyAdmitted", "Pet already admitted."));
        }

        if (await _dbContext.Hospitalizations.AnyAsync(h => h.BedId == bedId && h.Status == HospitalizationStatus.Admitted, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("Hospitalization.BedOccupied", "Bed occupied."));
        }

        var id = Guid.NewGuid();
        var admit = Hospitalization.Admit(id, petId, veterinarianId, bedId, reason, DateTimeOffset.UtcNow);
        if (admit.IsFailure)
        {
            return Result.Failure<Guid>(admit.Error);
        }

        _dbContext.Hospitalizations.Add(admit.Value);
        EnqueueOutbox("AdmitPetCommand", OutboxPayloadFactory.AdmitPet(petId, veterinarianId, bedId, reason, id, id));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(id);
    }

    public async Task<Result> DischargeAsync(Guid hospitalizationId, CancellationToken cancellationToken = default)
    {
        var hosp = await _dbContext.Hospitalizations.FirstOrDefaultAsync(h => h.Id == hospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(new Error("Hospitalization.NotFound", "Not found."));
        }

        var discharge = hosp.Discharge(DateTimeOffset.UtcNow);
        if (discharge.IsFailure)
        {
            return discharge;
        }

        EnqueueOutbox("DischargePetCommand", OutboxPayloadFactory.DischargePet(hospitalizationId, Guid.NewGuid()));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AdministerAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default)
    {
        var hosp = await _dbContext.Hospitalizations.Include(h => h.Administrations).FirstOrDefaultAsync(h => h.Id == hospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(new Error("Hospitalization.NotFound", "Not found."));
        }

        var actor = new Guid("11111111-1111-1111-1111-111111111111");
        var result = hosp.AdministerMedication(administrationId, actor, notes, DateTimeOffset.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        EnqueueOutbox("AdministerMedicationCommand", OutboxPayloadFactory.AdministerMedication(hospitalizationId, administrationId, notes, Guid.NewGuid()));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SkipAsync(Guid hospitalizationId, Guid administrationId, string? notes, CancellationToken cancellationToken = default)
    {
        var hosp = await _dbContext.Hospitalizations.Include(h => h.Administrations).FirstOrDefaultAsync(h => h.Id == hospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure(new Error("Hospitalization.NotFound", "Not found."));
        }

        var result = hosp.SkipMedication(administrationId, new Guid("11111111-1111-1111-1111-111111111111"), notes, DateTimeOffset.UtcNow);
        if (result.IsFailure)
        {
            return result;
        }

        EnqueueOutbox("SkipMedicationCommand", OutboxPayloadFactory.SkipMedication(hospitalizationId, administrationId, notes, Guid.NewGuid()));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<HospitalizationDetailViewModel>> GetDetailAsync(Guid hospitalizationId, CancellationToken cancellationToken = default)
    {
        var hosp = await _dbContext.Hospitalizations.AsNoTracking()
            .Include(h => h.MedicationOrders)
            .Include(h => h.ProgressNotes)
            .Include(h => h.Procedures)
            .FirstOrDefaultAsync(h => h.Id == hospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<HospitalizationDetailViewModel>(new Error("Hospitalization.NotFound", "Not found."));
        }

        var pet = await _dbContext.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == hosp.PetId, cancellationToken);
        return Result.Success(new HospitalizationDetailViewModel
        {
            Id = hosp.Id,
            PetId = hosp.PetId,
            PetName = pet?.Name ?? string.Empty,
            BedId = hosp.BedId,
            Reason = hosp.Reason,
            AdmittedAt = hosp.AdmittedAt,
            Status = hosp.Status.ToString(),
            MedicationOrders = hosp.MedicationOrders.Select(o => new MedicationOrderViewModel
            {
                Id = o.Id,
                MedicationName = o.MedicationName,
                Dose = o.Dose,
                Route = o.Route,
                Status = o.Status.ToString()
            }).ToList(),
            ProgressNotes = hosp.ProgressNotes.OrderByDescending(n => n.RecordedAt).Select(n => new ProgressNoteViewModel
            {
                Id = n.Id,
                Text = n.Text,
                RecordedAt = n.RecordedAt
            }).ToList(),
            Procedures = hosp.Procedures.OrderByDescending(p => p.PerformedAt).Select(p => new HospitalProcedureViewModel
            {
                Id = p.Id,
                Name = p.Name,
                PerformedAt = p.PerformedAt
            }).ToList()
        });
    }

    public async Task<Result<Guid>> CreateMedicationOrderAsync(
        Guid hospitalizationId,
        string medicationName,
        string dose,
        string route,
        IReadOnlyList<TimeOnly> dailyTimes,
        DateOnly startsOn,
        DateOnly endsOn,
        CancellationToken cancellationToken = default)
    {
        var hosp = await _dbContext.Hospitalizations
            .Include(h => h.MedicationOrders)
            .Include(h => h.Administrations)
            .FirstOrDefaultAsync(h => h.Id == hospitalizationId, cancellationToken);
        if (hosp is null)
        {
            return Result.Failure<Guid>(new Error("Hospitalization.NotFound", "Not found."));
        }

        var orderId = Guid.NewGuid();
        var add = hosp.AddMedicationOrder(orderId, medicationName, dose, route, dailyTimes, startsOn, endsOn);
        if (add.IsFailure)
        {
            return Result.Failure<Guid>(add.Error);
        }

        EnqueueOutbox("CreateMedicationOrderCommand", OutboxPayloadFactory.CreateMedicationOrder(
            hospitalizationId, medicationName, dose, route, dailyTimes, startsOn, endsOn, orderId, Guid.NewGuid()));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(orderId);
    }

    private void EnqueueOutbox(string type, string payload)
    {
        var id = Guid.NewGuid();
        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
            Type = type,
            Payload = payload
        });
    }
}
