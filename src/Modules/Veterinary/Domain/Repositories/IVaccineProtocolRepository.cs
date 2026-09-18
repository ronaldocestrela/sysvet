using Core.Domain.Entities;
using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for tenant vaccine protocol catalog.</summary>
public interface IVaccineProtocolRepository
{
    /// <summary>Loads a protocol with dose lines.</summary>
    Task<VaccineProtocol?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists active protocols optionally filtered by species.</summary>
    Task<List<VaccineProtocol>> ListAsync(PetSpecies? species, bool activeOnly, CancellationToken cancellationToken = default);

    /// <summary>Finds a dose line across protocols (for registration).</summary>
    Task<(VaccineProtocol Protocol, VaccineProtocolDose Dose)?> FindDoseAsync(Guid protocolDoseId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new protocol.</summary>
    Task AddAsync(VaccineProtocol protocol, CancellationToken cancellationToken = default);

    /// <summary>Marks an existing protocol as modified.</summary>
    void Update(VaccineProtocol protocol);
}
