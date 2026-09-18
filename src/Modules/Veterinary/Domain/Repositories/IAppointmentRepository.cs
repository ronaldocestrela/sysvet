using Core.Domain;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Enums;

namespace Veterinary.Domain.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task AddAsync(Appointment entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<Appointment>> GetByVeterinarianAndDateAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);

    /// <summary>Appointments on the calendar day; all vets when <paramref name="veterinarianId"/> is null.</summary>
    Task<IEnumerable<Appointment>> GetByDayAsync(Guid? veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);

    /// <summary>Detects active appointments overlapping the interval.</summary>
    Task<bool> HasOverlappingAsync(
        Guid veterinarianId,
        DateTimeOffset start,
        int durationMinutes,
        Guid? excludeAppointmentId,
        CancellationToken cancellationToken = default);
}
