namespace Core.Application.Sync;

/// <summary>
/// Outcome of processing a push batch with stop-on-first-error semantics.
/// </summary>
public sealed class SyncPushResult
{
    /// <summary>Outbox message ids successfully applied before any failure.</summary>
    public IReadOnlyList<Guid> ProcessedIds { get; init; } = Array.Empty<Guid>();

    /// <summary>First message that failed, if any.</summary>
    public Guid? FailedMessageId { get; init; }

    /// <summary>Domain error code when <see cref="FailedMessageId"/> is set.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Human-readable failure message.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>When true, the client should dead-letter instead of retrying.</summary>
    public bool IsPermanentFailure { get; init; }
}
