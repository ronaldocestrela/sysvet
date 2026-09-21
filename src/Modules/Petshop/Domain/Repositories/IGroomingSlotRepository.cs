using Core.Domain;
using Petshop.Domain.Entities;

namespace Petshop.Domain.Repositories;

/// <summary>
/// Persistence port for groomer schedule slots.
/// </summary>
public interface IGroomingSlotRepository : IRepository<GroomingSlot>
{
    Task<IEnumerable<GroomingSlot>> GetAvailableSlotsAsync(Guid groomerId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task<IEnumerable<GroomingSlot>> GetAllSlotsForDayAsync(Guid groomerId, DateTimeOffset date, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<GroomingSlot> slots, CancellationToken cancellationToken = default);
}
