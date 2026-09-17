using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF repository over <see cref="OfflineDbContext"/> implementing Core domain ports.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
public abstract class OfflineRepositoryBase<T> : IRepository<T> where T : Entity
{
    /// <summary>Local SQLite context.</summary>
    protected readonly OfflineDbContext DbContext;

    /// <summary>Creates a repository for the given context.</summary>
    protected OfflineRepositoryBase(OfflineDbContext dbContext)
    {
        DbContext = dbContext;
    }

    /// <inheritdoc />
    public void Add(T entity) => DbContext.Set<T>().Add(entity);

    /// <inheritdoc />
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbContext.Set<T>().FindAsync([id], cancellationToken);

    /// <inheritdoc />
    public void Remove(T entity) => DbContext.Set<T>().Remove(entity);

    /// <inheritdoc />
    public void Update(T entity) => DbContext.Set<T>().Update(entity);
}
