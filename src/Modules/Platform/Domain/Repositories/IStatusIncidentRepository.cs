using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Persistence port for public status incidents (ADR-060).</summary>
public interface IStatusIncidentRepository
{
    /// <summary>Gets incident by id.</summary>
    Task<StatusIncident?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Lists open incidents ordered by start time descending.</summary>
    Task<IReadOnlyList<StatusIncident>> ListOpenAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists recent incidents for Super Admin (open first).</summary>
    Task<IReadOnlyList<StatusIncident>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>Persists a new incident.</summary>
    Task AddAsync(StatusIncident incident, CancellationToken cancellationToken = default);
}
