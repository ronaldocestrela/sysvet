using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.ValueObjects;

namespace Clients.Infrastructure.Crm;

/// <summary>SQLite-backed medical record store with sync outbox.</summary>
public sealed class OfflineMedicalRecordStore : IMedicalRecordStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineMedicalRecordStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MedicalRecordTimelineItemDto>>> GetTimelineByPetAsync(Guid petId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.MedicalRecords
            .Where(r => r.PetId == petId)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync(cancellationToken);

        var items = new List<MedicalRecordTimelineItemDto>();
        foreach (var record in records)
        {
            var appointment = await _dbContext.Appointments.FindAsync([record.AppointmentId], cancellationToken);
            items.Add(new MedicalRecordTimelineItemDto
            {
                Id = record.Id,
                AppointmentId = record.AppointmentId,
                OccurredAt = appointment?.Date ?? record.UpdatedAt,
                Status = record.Status.ToString(),
                AnamnesisPreview = Truncate(record.Anamnesis, 120),
                DiagnosisPreview = Truncate(record.Diagnosis, 120)
            });
        }

        return Result.Success<IReadOnlyList<MedicalRecordTimelineItemDto>>(items);
    }

    /// <inheritdoc />
    public async Task<Result<MedicalRecordDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.MedicalRecords
            .Include(r => r.EvolutionNotes)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (record is null)
        {
            return Result.Failure<MedicalRecordDetailDto>(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        return Result.Success(MapDetail(record));
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> GetOrCreateByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId, cancellationToken);
        if (existing is not null)
        {
            return Result.Success(existing.Id);
        }

        var appointment = await _dbContext.Appointments.FindAsync([appointmentId], cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<Guid>(new Error("Appointment.NotFound", "Appointment not found locally."));
        }

        if (!appointment.IsEligibleForMedicalRecord())
        {
            return Result.Failure<Guid>(new Error("MedicalRecord.AppointmentNotEligible", "Appointment is not eligible for a medical record."));
        }

        var created = MedicalRecord.Create(
            Guid.NewGuid(),
            appointment.Id,
            appointment.VeterinarianId,
            appointment.TutorId,
            appointment.PetId);

        if (created.IsFailure)
        {
            return Result.Failure<Guid>(created.Error);
        }

        _dbContext.MedicalRecords.Add(created.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }

    /// <inheritdoc />
    public async Task<Result> UpdateAnamnesisAsync(Guid medicalRecordId, string anamnesis, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var update = record.SetAnamnesis(anamnesis);
        if (update.IsFailure)
        {
            return update;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> RecordVitalSignsAsync(Guid medicalRecordId, VitalSignsInputDto vitals, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var vitalsResult = VitalSigns.Create(
            vitals.WeightKg,
            vitals.TemperatureC,
            vitals.HeartRateBpm,
            vitals.RespiratoryRateBpm,
            vitals.MucousMembranes,
            vitals.CapillaryRefillTime,
            vitals.MeasuredAt);

        if (vitalsResult.IsFailure)
        {
            return Result.Failure(vitalsResult.Error);
        }

        var update = record.RecordVitalSigns(vitalsResult.Value);
        if (update.IsFailure)
        {
            return update;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddEvolutionNoteAsync(Guid medicalRecordId, string text, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure<Guid>(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var noteId = Guid.NewGuid();
        var add = record.AddEvolutionNote(noteId, Guid.Empty, text, DateTimeOffset.UtcNow);
        if (add.IsFailure)
        {
            return add;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(noteId);
    }

    /// <inheritdoc />
    public async Task<Result> SetDiagnosisAsync(Guid medicalRecordId, string diagnosis, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var update = record.SetDiagnosis(diagnosis);
        if (update.IsFailure)
        {
            return Result.Failure(update.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> SetConductAsync(Guid medicalRecordId, string conduct, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var update = record.SetConduct(conduct);
        if (update.IsFailure)
        {
            return Result.Failure(update.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<Result> FinalizeAsync(Guid medicalRecordId, CancellationToken cancellationToken = default)
    {
        var record = await FindRecordAsync(medicalRecordId, cancellationToken);
        if (record is null)
        {
            return Result.Failure(new Error("MedicalRecord.NotFound", "Medical record not found locally."));
        }

        var finalize = record.FinalizeRecord();
        if (finalize.IsFailure)
        {
            return Result.Failure(finalize.Error);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<MedicalRecord?> FindRecordAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.MedicalRecords.Include(r => r.EvolutionNotes).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    private static MedicalRecordDetailDto MapDetail(MedicalRecord record) =>
        new()
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            PetId = record.PetId,
            Anamnesis = record.Anamnesis,
            Diagnosis = record.Diagnosis,
            Conduct = record.Prescription,
            Status = record.Status.ToString(),
            VitalSigns = record.VitalSigns is null
                ? null
                : new VitalSignsInputDto
                {
                    WeightKg = record.VitalSigns.WeightKg,
                    TemperatureC = record.VitalSigns.TemperatureC,
                    HeartRateBpm = record.VitalSigns.HeartRateBpm,
                    RespiratoryRateBpm = record.VitalSigns.RespiratoryRateBpm,
                    MucousMembranes = record.VitalSigns.MucousMembranes,
                    CapillaryRefillTime = record.VitalSigns.CapillaryRefillTime,
                    MeasuredAt = record.VitalSigns.MeasuredAt
                },
            EvolutionNotes = record.EvolutionNotes
                .OrderBy(n => n.RecordedAt)
                .Select(n => new EvolutionNoteItemDto { Id = n.Id, Text = n.Text, RecordedAt = n.RecordedAt })
                .ToList()
        };

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
