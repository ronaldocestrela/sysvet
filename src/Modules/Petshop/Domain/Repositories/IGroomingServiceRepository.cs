using Core.Domain;
using Petshop.Domain.Entities;

namespace Petshop.Domain.Repositories;

/// <summary>
/// Persistence port for salon service catalog.
/// </summary>
public interface IGroomingServiceRepository : IRepository<GroomingService>
{
    Task AddAsync(GroomingService entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroomingService>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroomingService>> ListAllAsync(CancellationToken cancellationToken = default);
}
