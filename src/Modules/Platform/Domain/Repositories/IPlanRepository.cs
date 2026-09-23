using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Catalog plan persistence port.</summary>
public interface IPlanRepository
{
    /// <summary>Lists all plans with included modules.</summary>
    Task<IReadOnlyList<Plan>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets plan by id with modules.</summary>
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets plan by unique code.</summary>
    Task<Plan?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Adds a plan row.</summary>
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);

    /// <summary>True when code exists.</summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
