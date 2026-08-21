using Core.Domain;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task AddAsync(Appointment entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByVeterinarianAndDateAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);
}
