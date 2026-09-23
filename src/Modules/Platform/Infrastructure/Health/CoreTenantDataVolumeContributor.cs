using Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;

namespace Platform.Infrastructure.Health;

/// <summary>Counts Core CRM rows for tenant health volume (9.7).</summary>
public sealed class CoreTenantDataVolumeContributor : ITenantDataVolumeContributor
{
    private readonly CoreDbContext _coreDbContext;

    /// <summary>Creates the contributor.</summary>
    public CoreTenantDataVolumeContributor(CoreDbContext coreDbContext) => _coreDbContext = coreDbContext;

    /// <inheritdoc />
    public string ModuleName => "Core";

    /// <inheritdoc />
    public async Task<long> CountRowsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var tutors = await _coreDbContext.Tutors
            .IgnoreQueryFilters()
            .CountAsync(t => EF.Property<Guid>(t, "TenantId") == tenantId, cancellationToken);
        var pets = await _coreDbContext.Pets
            .IgnoreQueryFilters()
            .CountAsync(p => EF.Property<Guid>(p, "TenantId") == tenantId, cancellationToken);
        return tutors + pets;
    }
}
