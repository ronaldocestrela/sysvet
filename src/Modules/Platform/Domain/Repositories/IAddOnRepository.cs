using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Catalog add-on persistence port.</summary>
public interface IAddOnRepository
{
    /// <summary>Lists all add-ons.</summary>
    Task<IReadOnlyList<AddOn>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets add-on by id.</summary>
    Task<AddOn?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Gets add-on by code.</summary>
    Task<AddOn?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Adds an add-on row.</summary>
    Task AddAsync(AddOn addOn, CancellationToken cancellationToken = default);

    /// <summary>True when code exists.</summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
