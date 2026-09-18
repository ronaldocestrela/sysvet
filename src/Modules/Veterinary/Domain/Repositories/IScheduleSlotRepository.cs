using Core.Domain;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

public interface IScheduleSlotRepository : IRepository<ScheduleSlot>
{
    Task<IEnumerable<ScheduleSlot>> GetAvailableSlotsAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);

    /// <summary>All slots for a veterinarian on a calendar day (available and booked/blocked).</summary>
    Task<IEnumerable<ScheduleSlot>> GetAllSlotsForDayAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<ScheduleSlot> slots, CancellationToken cancellationToken = default);
}
