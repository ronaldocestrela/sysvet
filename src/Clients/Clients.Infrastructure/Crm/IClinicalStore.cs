using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Offline-first clinical artifacts (exams, prescriptions metadata, attachment list).</summary>
public interface IClinicalStore
{
    Task<Result<IReadOnlyList<ClinicalExamListItemDto>>> GetExamsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result<Guid>> RequestExamAsync(Guid appointmentId, string name, string category, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<IssuedPrescriptionListItemDto>>> GetPrescriptionsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<ClinicalAttachmentListItemDto>>> GetAttachmentsByAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}

public sealed class ClinicalExamListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ResultSummary { get; init; } = string.Empty;
}

public sealed class IssuedPrescriptionListItemDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ItemCount { get; init; }
}

public sealed class ClinicalAttachmentListItemDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
}
