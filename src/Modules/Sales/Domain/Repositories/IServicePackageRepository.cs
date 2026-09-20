using Sales.Domain.Entities;

namespace Sales.Domain.Repositories;

/// <summary>Persistence port for prepaid service package offers.</summary>
public interface IServicePackageRepository
{
    Task<ServicePackage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServicePackage>> ListAllAsync(CancellationToken cancellationToken = default);
    void Add(ServicePackage package);
    void Update(ServicePackage package);
}
