using Microsoft.EntityFrameworkCore;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Persistence.Repositories;

/// <summary>EF implementation of branch catalog repository.</summary>
public sealed class BranchRepository : IBranchRepository
{
    private readonly PlatformDbContext _context;

    /// <summary>Creates the repository.</summary>
    public BranchRepository(PlatformDbContext context) => _context = context;

    /// <inheritdoc />
    public Task<Branch?> GetByIdAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken = default) =>
        _context.Branches.FirstOrDefaultAsync(
            b => b.TenantId == tenantId && b.Id == branchId && b.DeletedAt == null,
            cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Branch>> ListByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        await _context.Branches
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId && b.DeletedAt == null)
            .OrderByDescending(b => b.IsHeadquarters)
            .ThenBy(b => b.LegalName)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> CnpjExistsAsync(
        Guid tenantId,
        string cnpjDigits,
        Guid? excludeBranchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Branches.Where(b => b.TenantId == tenantId && b.Cnpj == cnpjDigits && b.DeletedAt == null);
        if (excludeBranchId is { } exclude)
        {
            query = query.Where(b => b.Id != exclude);
        }

        return query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<Branch?> GetHeadquartersAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Branches.FirstOrDefaultAsync(
            b => b.TenantId == tenantId && b.IsHeadquarters && b.DeletedAt == null,
            cancellationToken);

    public Task<bool> HeadquartersExistsAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        _context.Branches.AnyAsync(
            b => b.TenantId == tenantId && b.IsHeadquarters && b.DeletedAt == null,
            cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default) =>
        await _context.Branches.AddAsync(branch, cancellationToken);

    /// <inheritdoc />
    public Task RemoveAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        _context.Branches.Remove(branch);
        return Task.CompletedTask;
    }
}
