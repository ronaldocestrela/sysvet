using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation for partner API keys.</summary>
public sealed class PartnerApiKeyRepository : IPartnerApiKeyRepository
{
    private readonly PlatformDbContext _dbContext;

    /// <summary>Creates the repository.</summary>
    public PartnerApiKeyRepository(PlatformDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public Task AddAsync(PartnerApiKey key, CancellationToken cancellationToken = default) =>
        _dbContext.PartnerApiKeys.AddAsync(key, cancellationToken).AsTask();

    /// <inheritdoc />
    public Task<PartnerApiKey?> GetBySecretHashAsync(string secretHash, CancellationToken cancellationToken = default) =>
        _dbContext.PartnerApiKeys.FirstOrDefaultAsync(k => k.SecretHash == secretHash, cancellationToken);

    /// <inheritdoc />
    public Task<PartnerApiKey?> GetByIdAsync(Guid tenantId, Guid keyId, CancellationToken cancellationToken = default) =>
        _dbContext.PartnerApiKeys.FirstOrDefaultAsync(k => k.TenantId == tenantId && k.Id == keyId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PartnerApiKey>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _dbContext.PartnerApiKeys
            .AsNoTracking()
            .Where(k => k.TenantId == tenantId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
}
