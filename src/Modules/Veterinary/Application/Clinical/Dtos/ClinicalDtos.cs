namespace Veterinary.Application.Clinical.Dtos;

/// <summary>Prescription template list/detail DTO.</summary>
public sealed class PrescriptionTemplateDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<PrescriptionLineDto> Items { get; init; } = Array.Empty<PrescriptionLineDto>();
}

/// <summary>Shared medication line shape.</summary>
public sealed class PrescriptionLineDto
{
    public Guid Id { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Concentration { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string Frequency { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string Instructions { get; init; } = string.Empty;
    public int SortOrder { get; init; }
}

/// <summary>Issued prescription document.</summary>
public sealed class IssuedPrescriptionDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid? TemplateId { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<PrescriptionLineDto> Items { get; init; } = Array.Empty<PrescriptionLineDto>();
}

/// <summary>Clinical exam row.</summary>
public sealed class ClinicalExamDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ResultSummary { get; init; } = string.Empty;
}

/// <summary>Attachment metadata (no binary).</summary>
public sealed class ClinicalAttachmentDto
{
    public Guid Id { get; init; }
    public Guid AppointmentId { get; init; }
    public Guid? MedicalRecordId { get; init; }
    public Guid? ClinicalExamId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string Kind { get; init; } = string.Empty;
}
