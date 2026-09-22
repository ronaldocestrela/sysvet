using Commerce.Domain.Enums;
using Core.Domain;

namespace Commerce.Domain.Entities;

/// <summary>
/// Durable outbox job to push listings or pull marketplace orders (ADR-045 PoC).
/// </summary>
public sealed class MarketplaceSyncJob : AggregateRoot
{
    public const int DefaultMaxAttempts = 5;

    public MarketplaceSyncJobKind Kind { get; private set; }
    public MarketplaceSyncJobStatus Status { get; private set; }
    public Guid? ProductOfferId { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }

#pragma warning disable CS8618
    private MarketplaceSyncJob() : base(Guid.Empty) { }
#pragma warning restore CS8618

    /// <summary>Enqueues a pending sync job.</summary>
    public static Result<MarketplaceSyncJob> Enqueue(
        MarketplaceSyncJobKind kind,
        string payloadJson,
        string idempotencyKey,
        Guid? productOfferId = null,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return Result.Failure<MarketplaceSyncJob>(new Error("Commerce.SyncJob.InvalidPayload", "Payload obrigatório."));
        }

        var clock = now ?? DateTimeOffset.UtcNow;
        return Result.Success(new MarketplaceSyncJob
        {
            Id = Guid.NewGuid(),
            Kind = kind,
            Status = MarketplaceSyncJobStatus.Pending,
            ProductOfferId = productOfferId,
            PayloadJson = payloadJson,
            IdempotencyKey = idempotencyKey ?? string.Empty,
            AttemptCount = 0,
            MaxAttempts = DefaultMaxAttempts,
            NextAttemptAt = clock
        });
    }

    /// <summary>Marks job processing attempt.</summary>
    public void MarkProcessing()
    {
        Status = MarketplaceSyncJobStatus.Processing;
        AttemptCount++;
    }

    /// <summary>Marks success.</summary>
    public void MarkSucceeded()
    {
        Status = MarketplaceSyncJobStatus.Succeeded;
        LastError = null;
    }

    /// <summary>Schedules retry or dead-letter.</summary>
    public void MarkFailed(string error, DateTimeOffset now)
    {
        LastError = error;
        if (AttemptCount >= MaxAttempts)
        {
            Status = MarketplaceSyncJobStatus.DeadLetter;
            return;
        }

        Status = MarketplaceSyncJobStatus.Pending;
        var delaySeconds = 30 * Math.Pow(2, AttemptCount - 1);
        NextAttemptAt = now.AddSeconds(delaySeconds);
    }
}
