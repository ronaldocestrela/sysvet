using Microsoft.EntityFrameworkCore;
using Veterinary.Domain.Entities;
using Veterinary.Domain.Repositories;

namespace Veterinary.Infrastructure.Persistence.Repositories;

public sealed class WardUnitRepository : IWardUnitRepository
{
    private readonly VeterinaryDbContext _dbContext;

    public WardUnitRepository(VeterinaryDbContext dbContext) => _dbContext = dbContext;

    public async Task AddAsync(WardUnit unit, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WardUnit>().AddAsync(unit, cancellationToken);

    public async Task<WardUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<WardUnit>()
            .Include(u => u.Beds)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<List<WardUnit>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<WardUnit>().AsNoTracking().Include(u => u.Beds).AsQueryable();
        if (activeOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query.OrderBy(u => u.Name).ToListAsync(cancellationToken);
    }

    public async Task<(WardUnit Unit, Bed Bed)?> FindBedAsync(Guid bedId, CancellationToken cancellationToken = default)
    {
        var bed = await _dbContext.Set<Bed>().AsNoTracking().FirstOrDefaultAsync(b => b.Id == bedId, cancellationToken);
        if (bed is null)
        {
            return null;
        }

        var unit = await GetByIdAsync(bed.WardUnitId, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        var matched = unit.Beds.FirstOrDefault(b => b.Id == bedId);
        return matched is null ? null : (unit, matched);
    }

    public void Update(WardUnit unit) => _dbContext.Set<WardUnit>().Update(unit);
}
