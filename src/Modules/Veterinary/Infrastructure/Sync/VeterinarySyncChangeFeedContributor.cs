using Core.Application.Sync;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Infrastructure.Persistence;

namespace Veterinary.Infrastructure.Sync;

/// <summary>
/// Exposes veterinary agenda rows to the Core sync pull feed.
/// </summary>
public sealed class VeterinarySyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly VeterinaryDbContext _dbContext;

    /// <summary>Creates the contributor.</summary>
    public VeterinarySyncChangeFeedContributor(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        var appointments = await _dbContext.Appointments.AsNoTracking().ToListAsync(cancellationToken);
        var appointmentCandidates = appointments
            .Where(a => a.UpdatedAt > since)
            .OrderBy(a => a.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreAppointments = appointmentCandidates.Count > take;
        if (hasMoreAppointments)
        {
            appointmentCandidates = appointmentCandidates.Take(take).ToList();
        }

        var slots = await _dbContext.ScheduleSlots.AsNoTracking().ToListAsync(cancellationToken);
        var slotCandidates = slots
            .Where(s => s.UpdatedAt > since)
            .OrderBy(s => s.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreSlots = slotCandidates.Count > take;
        if (hasMoreSlots)
        {
            slotCandidates = slotCandidates.Take(take).ToList();
        }

        var maxUpdated = since;
        foreach (var a in appointmentCandidates)
        {
            if (a.UpdatedAt > maxUpdated)
            {
                maxUpdated = a.UpdatedAt;
            }
        }

        foreach (var s in slotCandidates)
        {
            if (s.UpdatedAt > maxUpdated)
            {
                maxUpdated = s.UpdatedAt;
            }
        }

        var medicalRecords = await _dbContext.MedicalRecords
            .AsNoTracking()
            .Include(m => m.EvolutionNotes)
            .ToListAsync(cancellationToken);
        var recordCandidates = medicalRecords
            .Where(r => r.UpdatedAt > since)
            .OrderBy(r => r.UpdatedAt)
            .Take(take + 1)
            .ToList();

        var hasMoreRecords = recordCandidates.Count > take;
        if (hasMoreRecords)
        {
            recordCandidates = recordCandidates.Take(take).ToList();
        }

        foreach (var r in recordCandidates)
        {
            if (r.UpdatedAt > maxUpdated)
            {
                maxUpdated = r.UpdatedAt;
            }
        }

        return new SyncContributorChanges
        {
            Appointments = appointmentCandidates.Select(MapAppointment).ToList(),
            ScheduleSlots = slotCandidates.Select(MapSlot).ToList(),
            MedicalRecords = recordCandidates.Select(MapMedicalRecord).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = hasMoreAppointments || hasMoreSlots || hasMoreRecords
        };
    }

    private static SyncAppointmentDto MapAppointment(Appointment appointment) =>
        new()
        {
            Id = appointment.Id,
            TutorId = appointment.TutorId,
            PetId = appointment.PetId,
            VeterinarianId = appointment.VeterinarianId,
            Date = appointment.Date,
            DurationInMinutes = appointment.DurationInMinutes,
            Reason = appointment.Reason,
            Status = appointment.Status.ToString(),
            UpdatedAt = appointment.UpdatedAt,
            RowVersion = Convert.ToBase64String(appointment.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncScheduleSlotDto MapSlot(ScheduleSlot slot) =>
        new()
        {
            Id = slot.Id,
            VeterinarianId = slot.VeterinarianId,
            Date = slot.Date,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            IsAvailable = slot.IsAvailable,
            UpdatedAt = slot.UpdatedAt,
            RowVersion = Convert.ToBase64String(slot.RowVersion ?? Array.Empty<byte>())
        };

    private static SyncMedicalRecordDto MapMedicalRecord(MedicalRecord record) =>
        new()
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            VeterinarianId = record.VeterinarianId,
            TutorId = record.TutorId,
            PetId = record.PetId,
            Anamnesis = record.Anamnesis,
            Diagnosis = record.Diagnosis,
            Prescription = record.Prescription,
            Status = record.Status.ToString(),
            VitalWeightKg = record.VitalSigns?.WeightKg,
            VitalTemperatureC = record.VitalSigns?.TemperatureC,
            VitalHeartRateBpm = record.VitalSigns?.HeartRateBpm,
            VitalRespiratoryRateBpm = record.VitalSigns?.RespiratoryRateBpm,
            VitalMucousMembranes = record.VitalSigns?.MucousMembranes,
            VitalCapillaryRefillTime = record.VitalSigns?.CapillaryRefillTime,
            VitalMeasuredAt = record.VitalSigns?.MeasuredAt,
            EvolutionNotes = record.EvolutionNotes.Select(n => new SyncEvolutionNoteDto
            {
                Id = n.Id,
                AuthorId = n.AuthorId,
                Text = n.Text,
                RecordedAt = n.RecordedAt
            }).ToList(),
            UpdatedAt = record.UpdatedAt,
            RowVersion = Convert.ToBase64String(record.RowVersion ?? Array.Empty<byte>())
        };
}
