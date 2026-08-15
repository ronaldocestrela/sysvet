using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

public abstract class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly CoreDbContext _dbContext;

    protected Repository(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(T entity) => _dbContext.Set<T>().Add(entity);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().ToListAsync(cancellationToken);

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<T>().FindAsync(new object[] { id }, cancellationToken);

    public void Remove(T entity) => _dbContext.Set<T>().Remove(entity);

    public void Update(T entity) => _dbContext.Set<T>().Update(entity);
}
