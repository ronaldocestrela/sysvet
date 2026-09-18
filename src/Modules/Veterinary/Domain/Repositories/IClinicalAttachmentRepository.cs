using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for clinical attachment metadata.</summary>
public interface IClinicalAttachmentRepository
{
    Task<ClinicalAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalAttachment>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task AddAsync(ClinicalAttachment attachment, CancellationToken cancellationToken = default);

    void Update(ClinicalAttachment attachment);
}
