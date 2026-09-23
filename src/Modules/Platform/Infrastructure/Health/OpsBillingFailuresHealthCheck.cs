using Core.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Platform.Domain.Entities;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Health;

/// <summary>Ops-only health check for outstanding failed SaaS invoices (10.5).</summary>
public sealed class OpsBillingFailuresHealthCheck : IHealthCheck
{
    private readonly PlatformDbContext _dbContext;
    private readonly ObservabilityOptions _options;

    /// <summary>Creates the check.</summary>
    public OpsBillingFailuresHealthCheck(PlatformDbContext dbContext, IOptions<ObservabilityOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var failedCount = await _dbContext.BillingInvoices
            .AsNoTracking()
            .CountAsync(i => i.Status == BillingInvoiceStatus.Failed, cancellationToken);

        if (failedCount >= _options.BillingFailedInvoiceThreshold)
        {
            return HealthCheckResult.Degraded($"{failedCount} failed billing invoice(s) outstanding.");
        }

        return HealthCheckResult.Healthy("Failed billing invoices within threshold.");
    }
}
