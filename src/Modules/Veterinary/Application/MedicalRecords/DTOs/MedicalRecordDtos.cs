namespace Veterinary.Application.MedicalRecords.DTOs;

/// <summary>API representation of vital signs captured during a visit.</summary>
public sealed class VitalSignsDto
{
    public decimal WeightKg { get; init; }
    public decimal TemperatureC { get; init; }
    public int? HeartRateBpm { get; init; }
    public int? RespiratoryRateBpm { get; init; }
    public string MucousMembranes { get; init; } = string.Empty;
    public string CapillaryRefillTime { get; init; } = string.Empty;
    public DateTimeOffset MeasuredAt { get; init; }
}

/// <summary>Single evolution entry on a medical record.</summary>
public sealed class EvolutionNoteDto
{
    public Guid Id { get; init; }
    public Guid AuthorId { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

/// <summary>Full consultation medical record for read/update flows.</summary>
public sealed class MedicalRecordDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public string Anamnesis { get; init; } = string.Empty;
    public VitalSignsDto? VitalSigns { get; init; }
    public string Diagnosis { get; init; } = string.Empty;
    public string Conduct { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? AppointmentDate { get; init; }
    public IReadOnlyList<EvolutionNoteDto> EvolutionNotes { get; init; } = Array.Empty<EvolutionNoteDto>();
}

/// <summary>Timeline row for a pet's clinical history.</summary>
public sealed class PetClinicalTimelineItemDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AnamnesisPreview { get; init; } = string.Empty;
    public string DiagnosisPreview { get; init; } = string.Empty;
}
