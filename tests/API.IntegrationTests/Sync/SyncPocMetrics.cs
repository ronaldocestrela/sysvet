namespace API.IntegrationTests.Sync;

/// <summary>
/// Metrics captured around a single <see cref="Clients.Infrastructure.Sync.SyncBackgroundWorker"/> cycle for PoC 3.6.
/// </summary>
public sealed class SyncPocMetrics
{
    /// <summary>Wall-clock duration of the sync cycle in milliseconds.</summary>
    public long ElapsedMs { get; init; }

    /// <summary>Outbox messages still pending (not processed, no error).</summary>
    public int PendingCount { get; init; }

    /// <summary>Outbox messages in dead-letter state.</summary>
    public int ErrorCount { get; init; }

    /// <summary>Outbox messages successfully processed in this cycle or before.</summary>
    public int ProcessedCount { get; init; }

    /// <summary>Formats metrics for test output and documentation.</summary>
    public string ToLogLine() =>
        $"[POC-METRICS] elapsed_ms={ElapsedMs} pending={PendingCount} errors={ErrorCount} processed={ProcessedCount}";
}
