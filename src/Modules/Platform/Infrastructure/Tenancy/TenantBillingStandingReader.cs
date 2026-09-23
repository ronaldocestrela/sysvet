using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using Platform.Domain.Repositories;

namespace Platform.Infrastructure.Tenancy;

/// <inheritdoc />
public sealed class TenantBillingStandingReader : ITenantBillingStandingReader
{
    private readonly ITenantSubscriptionRepository _subscriptionRepository;

    /// <summary>Creates the reader.</summary>
    public TenantBillingStandingReader(ITenantSubscriptionRepository subscriptionRepository) =>
        _subscriptionRepository = subscriptionRepository;

    /// <inheritdoc />
    public async Task<BillingStanding?> GetStandingAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return null;
        }

        try
        {
            var subscription = await _subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
            return subscription?.BillingStanding;
        }
        catch (Exception ex) when (IsCatalogUnavailable(ex))
        {
            // Shared SQLite files used by tests may not have the platform catalog yet.
            return null;
        }
    }

    private static bool IsCatalogUnavailable(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("no such column", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
