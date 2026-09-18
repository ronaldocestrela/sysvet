using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for clinical exams.</summary>
public interface IClinicalExamRepository
{
    Task<ClinicalExam?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalExam>> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicalExam>> GetByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);

    Task AddAsync(ClinicalExam exam, CancellationToken cancellationToken = default);

    void Update(ClinicalExam exam);
}
