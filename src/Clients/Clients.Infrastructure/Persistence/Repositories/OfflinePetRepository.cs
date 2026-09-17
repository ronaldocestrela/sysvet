using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Persistence.Repositories;

/// <summary>
/// SQLite implementation of <see cref="IPetRepository"/> on the client.
/// </summary>
public sealed class OfflinePetRepository : OfflineRepositoryBase<Pet>, IPetRepository
{
    /// <summary>Creates the repository.</summary>
    public OfflinePetRepository(OfflineDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Pet>> GetByTutorIdAsync(Guid tutorId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Pets.Where(p => p.TutorId == tutorId).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedList<Pet>> SearchAsync(
        int page,
        int pageSize,
        Guid? tutorId,
        string? nameFilter,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Pets.AsQueryable();

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
