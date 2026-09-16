using Core.Domain;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IAccessProfileRepository"/>.
/// </summary>
public sealed class AccessProfileRepository : Repository<AccessProfile>, IAccessProfileRepository
{
    public AccessProfileRepository(CoreDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<AccessProfile?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessProfiles
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccessProfile?> GetSystemProfileByBaseRoleAsync(string baseRole, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessProfiles
            .FirstOrDefaultAsync(p => p.IsSystem && p.BaseRole == baseRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedList<AccessProfile>> SearchAsync(
        int page,
        int pageSize,
        string? nameFilter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AccessProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            var filter = nameFilter.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{filter}%"));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<AccessProfile>(items, total);
    }

    /// <inheritdoc />
    public async Task<AccessProfile?> GetByIdForTenantAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                p => p.Id == id && EF.Property<Guid>(p, "TenantId") == tenantId,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AccessProfile?> GetSystemProfileByBaseRoleForTenantAsync(
        string baseRole,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                p => p.IsSystem && p.BaseRole == baseRole && EF.Property<Guid>(p, "TenantId") == tenantId,
                cancellationToken);
    }
}
