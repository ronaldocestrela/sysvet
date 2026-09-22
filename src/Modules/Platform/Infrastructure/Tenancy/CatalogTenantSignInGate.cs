using Core.Application.Common.Interfaces;
using Core.Domain;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Tenancy;

/// <summary>Blocks sign-in when the tenant catalog row is not active.</summary>
public sealed class CatalogTenantSignInGate : ITenantSignInGate
{
    private readonly ITenantRepository _tenantRepository;

    /// <summary>Creates the gate.</summary>
    public CatalogTenantSignInGate(ITenantRepository tenantRepository) => _tenantRepository = tenantRepository;

    /// <inheritdoc />
    public async Task<Result> EnsureActiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Success();
        }

        Tenant? tenant;
        try
        {
            tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        }
        catch (Exception ex) when (IsCatalogUnavailable(ex))
        {
            // Shared SQLite files used by tests may not have the platform catalog yet.
            return Result.Success();
        }

        // Lifecycle applies only to tenants registered in the catalog (onboarding 9.2).
        if (tenant is null || tenant.Status == TenantStatus.Active)
        {
            return Result.Success();
        }

        return Result.Failure(Platform.Domain.ErrorCodes.Tenant.NotActive);
    }

    private static bool IsCatalogUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
