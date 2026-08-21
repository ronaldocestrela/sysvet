using Core.Domain;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IScheduleSlotRepository : IRepository<ScheduleSlot>
{
    Task<IEnumerable<ScheduleSlot>> GetAvailableSlotsAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);
}
