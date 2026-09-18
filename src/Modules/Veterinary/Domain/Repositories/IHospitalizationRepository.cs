using Veterinary.Domain.Entities;

namespace Veterinary.Domain.Repositories;

/// <summary>Persistence port for inpatient stays.</summary>
public interface IHospitalizationRepository
{
    /// <summary>Loads a stay with clinical children.</summary>
    Task<Hospitalization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists admitted stays.</summary>
    Task<List<Hospitalization>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Finds active stay for a pet, if any.</summary>
    Task<Hospitalization?> GetActiveByPetAsync(Guid petId, CancellationToken cancellationToken = default);

    /// <summary>Finds active stay occupying a bed, if any.</summary>
    Task<Hospitalization?> GetActiveByBedAsync(Guid bedId, CancellationToken cancellationToken = default);

    /// <summary>Persists a new stay.</summary>
    Task AddAsync(Hospitalization hospitalization, CancellationToken cancellationToken = default);

    /// <summary>Marks stay modified.</summary>
    void Update(Hospitalization hospitalization);
}
