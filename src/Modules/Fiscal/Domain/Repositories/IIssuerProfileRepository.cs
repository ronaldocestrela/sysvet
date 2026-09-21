using Fiscal.Domain.Entities;

namespace Fiscal.Domain.Repositories;

/// <summary>Issuer profile persistence.</summary>
public interface IIssuerProfileRepository
{
    /// <summary>Gets the tenant issuer profile if configured.</summary>
    Task<IssuerProfile?> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a new issuer profile.</summary>
    void Add(IssuerProfile profile);

    /// <summary>Updates an existing profile.</summary>
    void Update(IssuerProfile profile);
}
