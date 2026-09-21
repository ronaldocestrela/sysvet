using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Infrastructure.Persistence.Repositories;

public sealed class GroomingSlotRepository : IGroomingSlotRepository
{
    private readonly PetshopDbContext _dbContext;

    public GroomingSlotRepository(PetshopDbContext dbContext) => _dbContext = dbContext;

    public void Add(GroomingSlot entity) => _dbContext.GroomingSlots.Add(entity);

    public async Task AddRangeAsync(IEnumerable<GroomingSlot> slots, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingSlots.AddRangeAsync(slots, cancellationToken);

    public async Task<IEnumerable<GroomingSlot>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingSlots.ToListAsync(cancellationToken);

    public async Task<GroomingSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingSlots.FindAsync([id], cancellationToken);

    public void Remove(GroomingSlot entity) => _dbContext.GroomingSlots.Remove(entity);

    public void Update(GroomingSlot entity) => _dbContext.GroomingSlots.Update(entity);

    public async Task<IEnumerable<GroomingSlot>> GetAvailableSlotsAsync(Guid groomerId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var slots = await _dbContext.GroomingSlots
            .Where(s => s.GroomerId == groomerId && s.IsAvailable)
            .ToListAsync(cancellationToken);
        return slots.Where(s => s.Date >= dayStart && s.Date < dayEnd);
    }

    public async Task<IEnumerable<GroomingSlot>> GetAllSlotsForDayAsync(Guid groomerId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        var (dayStart, dayEnd) = GetDayRange(date);
        var slots = await _dbContext.GroomingSlots
            .Where(s => s.GroomerId == groomerId)
            .ToListAsync(cancellationToken);
        return slots.Where(s => s.Date >= dayStart && s.Date < dayEnd);
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetDayRange(DateTimeOffset date)
    {
        var utcDay = DateTime.SpecifyKind(date.UtcDateTime.Date, DateTimeKind.Utc);
        var start = new DateTimeOffset(utcDay, TimeSpan.Zero);
        return (start, start.AddDays(1));
    }
}
