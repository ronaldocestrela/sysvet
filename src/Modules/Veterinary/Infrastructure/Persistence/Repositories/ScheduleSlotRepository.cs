using Core.Domain;
using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public class ScheduleSlotRepository : IScheduleSlotRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public ScheduleSlotRepository(VeterinaryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ScheduleSlot entity) => _dbContext.ScheduleSlots.Add(entity);

    public async Task<IEnumerable<ScheduleSlot>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleSlots.ToListAsync(cancellationToken);

    public async Task<ScheduleSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleSlots.FindAsync(new object[] { id }, cancellationToken);

    public void Remove(ScheduleSlot entity) => _dbContext.ScheduleSlots.Remove(entity);

    public void Update(ScheduleSlot entity) => _dbContext.ScheduleSlots.Update(entity);

    public async Task<IEnumerable<ScheduleSlot>> GetAvailableSlotsAsync(Guid veterinarianId, DateTimeOffset date, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ScheduleSlots
            .Where(s => s.VeterinarianId == veterinarianId && s.Date.Date == date.Date && s.IsAvailable)
            .ToListAsync(cancellationToken);
    }
}
