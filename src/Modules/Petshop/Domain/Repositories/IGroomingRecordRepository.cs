using Core.Domain;
using Petshop.Domain.Entities;

namespace Petshop.Domain.Repositories;

/// <summary>
/// Persistence port for grooming digital records.
/// </summary>
public interface IGroomingRecordRepository : IRepository<GroomingRecord>
{
    Task AddAsync(GroomingRecord entity, CancellationToken cancellationToken = default);

    Task<GroomingRecord?> GetByAppointmentIdAsync(Guid groomingAppointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroomingRecord>> ListByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
}
