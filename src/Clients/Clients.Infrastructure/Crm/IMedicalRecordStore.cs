using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Offline-first port for veterinary medical records.</summary>
public interface IMedicalRecordStore
{
    Task<Result<IReadOnlyList<MedicalRecordTimelineItemDto>>> GetTimelineByPetAsync(Guid petId, CancellationToken cancellationToken = default);

    Task<Result<MedicalRecordDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> GetOrCreateByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result> UpdateAnamnesisAsync(Guid medicalRecordId, string anamnesis, CancellationToken cancellationToken = default);

    Task<Result> RecordVitalSignsAsync(Guid medicalRecordId, VitalSignsInputDto vitals, CancellationToken cancellationToken = default);

    Task<Result<Guid>> AddEvolutionNoteAsync(Guid medicalRecordId, string text, CancellationToken cancellationToken = default);

    Task<Result> SetDiagnosisAsync(Guid medicalRecordId, string diagnosis, CancellationToken cancellationToken = default);

    Task<Result> SetConductAsync(Guid medicalRecordId, string conduct, CancellationToken cancellationToken = default);

    Task<Result> FinalizeAsync(Guid medicalRecordId, CancellationToken cancellationToken = default);
}

/// <summary>Timeline list item for SharedUI.</summary>
public sealed class MedicalRecordTimelineItemDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string AnamnesisPreview { get; init; } = string.Empty;
    public string DiagnosisPreview { get; init; } = string.Empty;
}

/// <summary>Detailed medical record for editing.</summary>
public sealed class MedicalRecordDetailDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string Anamnesis { get; init; } = string.Empty;
    public string Diagnosis { get; init; } = string.Empty;
    public string Conduct { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public VitalSignsInputDto? VitalSigns { get; init; }
    public IReadOnlyList<EvolutionNoteItemDto> EvolutionNotes { get; init; } = Array.Empty<EvolutionNoteItemDto>();
}

/// <summary>Evolution note row.</summary>
public sealed class EvolutionNoteItemDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public DateTimeOffset RecordedAt { get; init; }
}

/// <summary>Vital signs input/output.</summary>
public sealed class VitalSignsInputDto
{
    public decimal WeightKg { get; init; }
    public decimal TemperatureC { get; init; }
    public int? HeartRateBpm { get; init; }
    public int? RespiratoryRateBpm { get; init; }
    public string MucousMembranes { get; init; } = string.Empty;
    public string CapillaryRefillTime { get; init; } = string.Empty;
    public DateTimeOffset MeasuredAt { get; init; }
}
