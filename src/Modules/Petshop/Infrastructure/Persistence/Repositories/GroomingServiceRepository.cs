using Microsoft.EntityFrameworkCore;
using Petshop.Domain.Entities;
using Petshop.Domain.Repositories;

namespace Petshop.Infrastructure.Persistence.Repositories;

public sealed class GroomingServiceRepository : IGroomingServiceRepository
{
    private readonly PetshopDbContext _dbContext;

    public GroomingServiceRepository(PetshopDbContext dbContext) => _dbContext = dbContext;

    public void Add(GroomingService entity) => _dbContext.GroomingServices.Add(entity);

    public async Task AddAsync(GroomingService entity, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingServices.AddAsync(entity, cancellationToken);

    public async Task<IEnumerable<GroomingService>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingServices.Include(s => s.DefaultSupplies).ToListAsync(cancellationToken);

    public async Task<GroomingService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.GroomingServices
            .Include(s => s.DefaultSupplies)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<GroomingService>> ListActiveAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingServices
            .Include(s => s.DefaultSupplies)
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GroomingService>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.GroomingServices
            .Include(s => s.DefaultSupplies)
            .ToListAsync(cancellationToken);

    public void Remove(GroomingService entity) => _dbContext.GroomingServices.Remove(entity);

    public void Update(GroomingService entity) => _dbContext.GroomingServices.Update(entity);
}
