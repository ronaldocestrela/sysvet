using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation of the tenant catalog repository.</summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public TenantRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    /// <inheritdoc />
    public Task<Tenant?> GetBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default) =>
        _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == normalizedSlug, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Tenant>> ListAsync(CancellationToken cancellationToken = default) =>
        await _context.Tenants.AsNoTracking().OrderBy(t => t.Slug).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        await _context.Tenants.AddAsync(tenant, cancellationToken);
    }
}
