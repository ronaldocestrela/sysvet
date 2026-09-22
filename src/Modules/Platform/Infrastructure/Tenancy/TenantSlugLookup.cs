using Platform.Application.Tenancy;
using Platform.Domain.Repositories;
using Platform.Domain.ValueObjects;

namespace Platform.Infrastructure.Tenancy;

/// <summary>Resolves slugs via the platform catalog.</summary>
public sealed class TenantSlugLookup : ITenantSlugLookup
{
    private readonly ITenantRepository _repository;

    /// <summary>Creates the lookup service.</summary>
    public TenantSlugLookup(ITenantRepository repository) => _repository = repository;

    /// <inheritdoc />
    public async Task<Guid?> ResolveTenantIdBySlugAsync(string normalizedSlug, CancellationToken cancellationToken = default)
    {
        var slug = TenantSlug.Normalize(normalizedSlug);
        if (!TenantSlug.IsValid(slug))
        {
            return null;
        }

        var tenant = await _repository.GetBySlugAsync(slug, cancellationToken);
        return tenant?.Id;
    }
}
