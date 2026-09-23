using Core.Domain.Entitlements;
using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <inheritdoc />
public sealed class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public FeatureFlagRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<IReadOnlyList<FeatureFlag>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.FeatureFlags.Where(f => f.TenantId == tenantId).ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<FeatureFlag>)t.Result, cancellationToken);

    /// <inheritdoc />
    public Task<FeatureFlag?> GetAsync(Guid tenantId, CommercialModule module, CancellationToken cancellationToken = default) =>
        _context.FeatureFlags.FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Module == module, cancellationToken);

    /// <inheritdoc />
    public async Task UpsertAsync(FeatureFlag flag, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(flag.TenantId, flag.Module, cancellationToken);
        if (existing is null)
        {
            await _context.FeatureFlags.AddAsync(flag, cancellationToken);
            return;
        }

        existing.SetState(flag.State);
    }
}
