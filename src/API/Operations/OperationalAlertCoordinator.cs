using Core.Application.Operations;
using Core.Application.Sync;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions;
using System.Diagnostics.Metrics;

namespace API.Operations;

/// <summary>
/// In-process operational alert sink for HTTP 5xx, sync push failures and billing charge failures (10.5).
/// </summary>
public sealed class OperationalAlertCoordinator : ISyncPushObserver, IBillingChargeObserver
{
    private readonly ILogger<OperationalAlertCoordinator> _logger;
    private readonly Counter<long> _alertsRaised;
    private readonly OperationalAlertWindow _http5xxWindow;
    private readonly OperationalAlertWindow _syncPushWindow;

    /// <summary>Creates coordinator windows from configured thresholds.</summary>
    public OperationalAlertCoordinator(
        IOptions<ObservabilityOptions> options,
        IMeterFactory meterFactory,
        ILogger<OperationalAlertCoordinator> logger)
    {
        _logger = logger;
        var settings = options.Value;
        _http5xxWindow = new OperationalAlertWindow(TimeSpan.FromMinutes(1), settings.Http5xxPerMinuteThreshold);
        _syncPushWindow = new OperationalAlertWindow(TimeSpan.FromMinutes(1), settings.SyncPushFailuresPerMinuteThreshold);
        var meter = meterFactory.Create("SysVet.Operations");
        _alertsRaised = meter.CreateCounter<long>("operational.alerts.raised");
    }

    /// <summary>Sliding window for HTTP 5xx responses.</summary>
    public OperationalAlertWindow Http5xxWindow => _http5xxWindow;

    /// <summary>Sliding window for sync push failures.</summary>
    public OperationalAlertWindow SyncPushWindow => _syncPushWindow;

    /// <summary>Records an HTTP 5xx response for alerting.</summary>
    public void RecordHttp5xx()
    {
        if (_http5xxWindow.Record(DateTimeOffset.UtcNow))
        {
            Emit(OperationalAlertNames.Http5xx, "HTTP 5xx rate exceeded threshold.");
        }
    }

    /// <inheritdoc />
    public void OnPushFailure(Guid tenantId, Guid messageId, string errorCode, bool isPermanentFailure)
    {
        if (_syncPushWindow.Record(DateTimeOffset.UtcNow))
        {
            Emit(
                OperationalAlertNames.SyncPushFailure,
                $"Sync push failures exceeded threshold. tenant={tenantId} message={messageId} code={errorCode} permanent={isPermanentFailure}");
        }
    }

    /// <inheritdoc />
    public void OnChargeFailure(Guid tenantId, Guid invoiceId)
    {
        Emit(
            OperationalAlertNames.BillingChargeFailure,
            $"Billing charge failed. tenant={tenantId} invoice={invoiceId}");
    }

    private void Emit(string alertName, string detail)
    {
        _alertsRaised.Add(1, new KeyValuePair<string, object?>("alert", alertName));
        using (_logger.BeginScope(new Dictionary<string, object> { ["OperationalAlert"] = alertName }))
        {
            _logger.LogWarning("{Detail}", detail);
        }
    }
}
