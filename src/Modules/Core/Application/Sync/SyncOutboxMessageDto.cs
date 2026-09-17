namespace Core.Application.Sync;

/// <summary>
/// Client outbox row posted to the sync push endpoint (ADR-002).
/// </summary>
public sealed class SyncOutboxMessageDto
{
    /// <summary>Outbox message identifier; reused as command idempotency key.</summary>
    public Guid Id { get; set; }

    /// <summary>MediatR command type name (e.g. CreateTutorCommand).</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>JSON payload matching the target command record.</summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>Local enqueue time (FIFO ordering).</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
