using Veterinary.Application.MedicalRecords.DTOs;
using Veterinary.Domain.Entities;

namespace Veterinary.Application.MedicalRecords;

/// <summary>Maps domain medical records to application DTOs.</summary>
public static class MedicalRecordMapper
{
    /// <summary>Projects an aggregate to a detailed DTO.</summary>
    public static MedicalRecordDto ToDto(MedicalRecord record, DateTimeOffset? appointmentDate = null) =>
        new()
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            VeterinarianId = record.VeterinarianId,
            TutorId = record.TutorId,
            PetId = record.PetId,
            Anamnesis = record.Anamnesis,
            VitalSigns = record.VitalSigns is null
                ? null
                : new VitalSignsDto
                {
                    WeightKg = record.VitalSigns.WeightKg,
                    TemperatureC = record.VitalSigns.TemperatureC,
                    HeartRateBpm = record.VitalSigns.HeartRateBpm,
                    RespiratoryRateBpm = record.VitalSigns.RespiratoryRateBpm,
                    MucousMembranes = record.VitalSigns.MucousMembranes,
                    CapillaryRefillTime = record.VitalSigns.CapillaryRefillTime,
                    MeasuredAt = record.VitalSigns.MeasuredAt
                },
            Diagnosis = record.Diagnosis,
            Conduct = record.Prescription,
            FollowUpOn = record.FollowUpOn,
            Status = record.Status.ToString(),
            UpdatedAt = record.UpdatedAt,
            AppointmentDate = appointmentDate,
            EvolutionNotes = record.EvolutionNotes
                .OrderBy(n => n.RecordedAt)
                .Select(n => new EvolutionNoteDto
                {
                    Id = n.Id,
                    AuthorId = n.AuthorId,
                    Text = n.Text,
                    RecordedAt = n.RecordedAt
                })
                .ToList()
        };

    /// <summary>Builds a compact timeline item from a record.</summary>
    public static PetClinicalTimelineItemDto ToTimelineItem(MedicalRecord record, DateTimeOffset occurredAt) =>
        new()
        {
            Id = record.Id,
            AppointmentId = record.AppointmentId,
            OccurredAt = occurredAt,
            Status = record.Status.ToString(),
            AnamnesisPreview = Truncate(record.Anamnesis, 120),
            DiagnosisPreview = Truncate(record.Diagnosis, 120)
        };

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}
