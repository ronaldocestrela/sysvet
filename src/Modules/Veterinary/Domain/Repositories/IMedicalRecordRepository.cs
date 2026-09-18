using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for consultation medical records.</summary>
public interface IMedicalRecordRepository
{
    Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MedicalRecord>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);

    Task AddAsync(MedicalRecord medicalRecord, CancellationToken cancellationToken = default);

    void Update(MedicalRecord medicalRecord);
}
