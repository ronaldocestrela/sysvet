using Core.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Core.Infrastructure.HealthChecks;

/// <summary>
/// Verifies that the Core module database is reachable via EF Core.
/// Used by orchestrator readiness probes without exposing connection details in logs.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly CoreDbContext _dbContext;

    /// <summary>
    /// Creates a health check bound to the Core persistence context.
    /// </summary>
    /// <param name="dbContext">The Core database context resolved from the current request scope.</param>
    public DatabaseHealthCheck(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            return canConnect
                ? HealthCheckResult.Healthy("Core database connection succeeded.")
                : HealthCheckResult.Unhealthy("Core database connection failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Core database connection failed.", ex);
        }
    }
}
