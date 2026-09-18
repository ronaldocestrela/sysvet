using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for ward unit catalog.</summary>
public interface IWardUnitRepository
{
    /// <summary>Loads a unit with beds.</summary>
    Task<WardUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists units optionally filtered to active only.</summary>
    Task<List<WardUnit>> ListAsync(bool activeOnly, CancellationToken cancellationToken = default);

    /// <summary>Finds a bed across units.</summary>
    Task<(WardUnit Unit, Bed Bed)?> FindBedAsync(Guid bedId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new unit.</summary>
    Task AddAsync(WardUnit unit, CancellationToken cancellationToken = default);

    /// <summary>Marks unit modified.</summary>
    void Update(WardUnit unit);
}
