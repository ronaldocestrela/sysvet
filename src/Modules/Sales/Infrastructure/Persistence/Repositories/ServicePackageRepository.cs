using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

public sealed class ServicePackageRepository : IServicePackageRepository
{
    private readonly SalesDbContext _dbContext;

    public ServicePackageRepository(SalesDbContext dbContext) => _dbContext = dbContext;

    public async Task<ServicePackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.ServicePackages.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ServicePackage>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.ServicePackages.AsNoTracking().ToListAsync(cancellationToken);

    public void Add(ServicePackage package) => _dbContext.ServicePackages.Add(package);

    public void Update(ServicePackage package) => _dbContext.ServicePackages.Update(package);
}
