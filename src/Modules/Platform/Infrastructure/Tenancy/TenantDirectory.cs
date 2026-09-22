using Platform.Application.Tenancy;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Tenancy;

/// <summary>Lists tenants from the catalog for background workers.</summary>
public sealed class TenantDirectory : ITenantDirectory
{
    private readonly ITenantRepository _repository;

    /// <summary>Creates the directory service.</summary>
    public TenantDirectory(ITenantRepository repository) => _repository = repository;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TenantDirectoryEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _repository.ListAsync(cancellationToken);
        return tenants
            .Select(t => new TenantDirectoryEntry(t.Id, t.SchemaName, t.Slug))
            .ToList();
    }
}
