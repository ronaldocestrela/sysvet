using API.Operations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.HealthChecks;

/// <summary>Ops-only health check for sync push failure rate (10.5).</summary>
public sealed class OpsSyncPushHealthCheck : IHealthCheck
{
    private readonly OperationalAlertCoordinator _coordinator;

    /// <summary>Creates the check.</summary>
    public OpsSyncPushHealthCheck(OperationalAlertCoordinator coordinator) => _coordinator = coordinator;

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var degraded = _coordinator.SyncPushWindow.IsAboveThreshold(DateTimeOffset.UtcNow);
        return Task.FromResult(
            degraded
                ? HealthCheckResult.Degraded("Sync push failure rate above threshold.")
                : HealthCheckResult.Healthy("Sync push failure rate within threshold."));
    }
}
