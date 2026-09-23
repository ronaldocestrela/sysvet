using API.Operations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.HealthChecks;

/// <summary>Ops-only health check for HTTP 5xx rate (10.5).</summary>
public sealed class OpsHttp5xxHealthCheck : IHealthCheck
{
    private readonly OperationalAlertCoordinator _coordinator;

    /// <summary>Creates the check.</summary>
    public OpsHttp5xxHealthCheck(OperationalAlertCoordinator coordinator) => _coordinator = coordinator;

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var degraded = _coordinator.Http5xxWindow.IsAboveThreshold(DateTimeOffset.UtcNow);
        return Task.FromResult(
            degraded
                ? HealthCheckResult.Degraded("HTTP 5xx rate above threshold.")
                : HealthCheckResult.Healthy("HTTP 5xx rate within threshold."));
    }
}
