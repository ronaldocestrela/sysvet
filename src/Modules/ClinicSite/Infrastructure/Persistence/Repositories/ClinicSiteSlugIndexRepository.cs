using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF implementation of the global slug registry.
/// </summary>
public sealed class ClinicSiteSlugIndexRepository : IClinicSiteSlugIndexRepository
{
    private readonly ClinicSiteDbContext _dbContext;

    public ClinicSiteSlugIndexRepository(ClinicSiteDbContext dbContext) => _dbContext = dbContext;

    /// <inheritdoc />
    public Task<ClinicSiteSlugIndex?> GetBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default) =>
        _dbContext.ClinicSiteSlugIndexes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == normalizedSlug, cancellationToken);

    /// <inheritdoc />
    public Task<ClinicSiteSlugIndex?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _dbContext.ClinicSiteSlugIndexes
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(ClinicSiteSlugIndex index, CancellationToken cancellationToken = default) =>
        await _dbContext.ClinicSiteSlugIndexes.AddAsync(index, cancellationToken);

    /// <inheritdoc />
    public Task UpdateAsync(ClinicSiteSlugIndex index, CancellationToken cancellationToken = default)
    {
        _dbContext.ClinicSiteSlugIndexes.Update(index);
        return Task.CompletedTask;
    }
}
