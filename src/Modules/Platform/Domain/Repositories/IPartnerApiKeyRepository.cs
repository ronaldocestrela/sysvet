using Platform.Domain.Entities;

namespace Platform.Domain.Repositories;

/// <summary>Partner API key catalog persistence (9.7).</summary>
public interface IPartnerApiKeyRepository
{
    /// <summary>Persists a new API key.</summary>
    Task AddAsync(PartnerApiKey key, CancellationToken cancellationToken = default);

    /// <summary>Finds an active or revoked key by secret hash.</summary>
    Task<PartnerApiKey?> GetBySecretHashAsync(string secretHash, CancellationToken cancellationToken = default);

    /// <summary>Finds a key by id for a tenant.</summary>
    Task<PartnerApiKey?> GetByIdAsync(Guid tenantId, Guid keyId, CancellationToken cancellationToken = default);

    /// <summary>Lists keys for a tenant (metadata only).</summary>
    Task<IReadOnlyList<PartnerApiKey>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
