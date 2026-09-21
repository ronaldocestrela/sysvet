using Core.Domain;
using Petshop.Domain.Entities;

namespace Petshop.Domain.Repositories;

/// <summary>
/// Persistence port for grooming appointments.
/// </summary>
public interface IGroomingAppointmentRepository : IRepository<GroomingAppointment>
{
    Task AddAsync(GroomingAppointment entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<GroomingAppointment>> GetByDayAsync(Guid? groomerId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task<bool> HasOverlappingAsync(
        Guid groomerId,
        DateTimeOffset start,
        int durationMinutes,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken = default);
}
