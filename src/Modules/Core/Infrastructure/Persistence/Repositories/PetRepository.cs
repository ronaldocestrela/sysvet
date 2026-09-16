using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(CoreDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IEnumerable<Pet>> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pets
            .Where(p => p.TutorId == tutorId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedList<Pet>> SearchAsync(
        int page,
        int pageSize,
        Guid? tutorId,
        string? nameFilter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Pets.AsQueryable();

        if (tutorId.HasValue && tutorId.Value != Guid.Empty)
        {
            query = query.Where(p => p.TutorId == tutorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{nameFilter.Trim()}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Pet>(items, total);
    }
}
