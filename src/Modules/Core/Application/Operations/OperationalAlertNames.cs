namespace Core.Application.Operations;

/// <summary>Stable operational alert identifiers for logs, metrics and health checks (10.5).</summary>
public static class OperationalAlertNames
{
    /// <summary>HTTP 5xx responses exceeded the configured rate.</summary>
    public const string Http5xx = "Http5xx";

    /// <summary>Sync push batch returned a failed outbox message.</summary>
    public const string SyncPushFailure = "SyncPushFailure";

    /// <summary>Gateway charge failed and invoice was marked failed.</summary>
    public const string BillingChargeFailure = "BillingChargeFailure";
}
