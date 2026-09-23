using System.ComponentModel.DataAnnotations;

namespace Core.Infrastructure.Configuration;

/// <summary>Operational observability and alert thresholds (Fase 10.5).</summary>
public sealed class ObservabilityOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Observability";

    /// <summary>OTLP gRPC/HTTP endpoint; empty disables export.</summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>When true, traces and metrics are written to the console (Development).</summary>
    public bool ConsoleExporter { get; set; }

    /// <summary>Trace sampling ratio between 0 and 1 inclusive.</summary>
    [Range(0, 1)]
    public double TraceSampleRatio { get; set; } = 1.0;

    /// <summary>HTTP 5xx count per minute before ops health degrades.</summary>
    [Range(1, 10000)]
    public int Http5xxPerMinuteThreshold { get; set; } = 10;

    /// <summary>Sync push failures per minute before ops health degrades.</summary>
    [Range(1, 10000)]
    public int SyncPushFailuresPerMinuteThreshold { get; set; } = 5;

    /// <summary>Open failed SaaS invoices count before ops billing health degrades.</summary>
    [Range(1, 10000)]
    public int BillingFailedInvoiceThreshold { get; set; } = 1;
}
