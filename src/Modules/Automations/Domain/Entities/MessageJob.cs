using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Durable outbound message job processed by the Automations worker with retry semantics.
/// </summary>
public sealed class MessageJob : AggregateRoot
{
    public const int DefaultMaxAttempts = 5;
    private static readonly TimeSpan RetryBaseDelay = TimeSpan.FromSeconds(30);

    public Guid TenantId { get; private set; }
    public MessageChannel Channel { get; private set; }
    public string TemplateCode { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public MessageJobStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; }
    public DateTimeOffset NextAttemptAt { get; private set; }
    public string? LastError { get; private set; }
    public string? SourceType { get; private set; }
    public Guid? SourceId { get; private set; }

    private readonly List<JobAttemptLog> _attemptLogs = new();

    /// <summary>Delivery attempt history.</summary>
    public IReadOnlyCollection<JobAttemptLog> AttemptLogs => _attemptLogs.AsReadOnly();

    private MessageJob() { }

    /// <summary>
    /// Enqueues a new pending job ready for the worker.
    /// </summary>
    public static Result<MessageJob> Enqueue(
        Guid tenantId,
        MessageChannel channel,
        string templateCode,
        string payloadJson,
        string idempotencyKey,
        string? sourceType = null,
        Guid? sourceId = null,
        int maxAttempts = DefaultMaxAttempts,
        Guid? id = null,
        DateTimeOffset? now = null,
        DateTimeOffset? nextAttemptAt = null)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<MessageJob>(new Error("MessageJob.InvalidTenant", "TenantId é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(templateCode))
        {
            return Result.Failure<MessageJob>(ErrorCodes.Template.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return Result.Failure<MessageJob>(ErrorCodes.Job.InvalidPayload);
        }

        var clock = now ?? DateTimeOffset.UtcNow;
        return Result.Success(new MessageJob
        {
            Id = id ?? Guid.NewGuid(),
            TenantId = tenantId,
            Channel = channel,
            TemplateCode = templateCode.Trim(),
            PayloadJson = payloadJson,
            IdempotencyKey = idempotencyKey ?? string.Empty,
            Status = MessageJobStatus.Pending,
            AttemptCount = 0,
            MaxAttempts = maxAttempts <= 0 ? DefaultMaxAttempts : maxAttempts,
            NextAttemptAt = nextAttemptAt ?? clock,
            SourceType = sourceType,
            SourceId = sourceId
        });
    }

    /// <summary>
    /// Postpones delivery without counting as a failed attempt (e.g. outside business hours).
    /// </summary>
    public void DeferUntil(DateTimeOffset when)
    {
        if (Status is MessageJobStatus.Succeeded or MessageJobStatus.DeadLetter)
        {
            return;
        }

        Status = MessageJobStatus.Pending;
        NextAttemptAt = when;
    }

    /// <summary>
    /// Marks the job as claimed by the worker for processing.
    /// </summary>
    public void Claim(DateTimeOffset now)
    {
        Status = MessageJobStatus.Processing;
        NextAttemptAt = now;
    }

    /// <summary>
    /// Records a successful delivery and closes the job.
    /// </summary>
    public void MarkSucceeded(string? detail, DateTimeOffset startedAt, DateTimeOffset finishedAt)
    {
        AttemptCount++;
        Status = MessageJobStatus.Succeeded;
        LastError = null;
        _attemptLogs.Add(JobAttemptLog.Create(Id, AttemptCount, AttemptOutcome.Success, detail, startedAt, finishedAt));
    }

    /// <summary>
    /// Schedules another attempt with exponential backoff or moves to dead-letter when exhausted.
    /// </summary>
    public void ScheduleRetry(string error, DateTimeOffset startedAt, DateTimeOffset finishedAt, DateTimeOffset now)
    {
        AttemptCount++;
        _attemptLogs.Add(JobAttemptLog.Create(Id, AttemptCount, AttemptOutcome.Failed, error, startedAt, finishedAt));
        LastError = error;

        if (AttemptCount >= MaxAttempts)
        {
            Status = MessageJobStatus.DeadLetter;
            return;
        }

        Status = MessageJobStatus.Failed;
        var delay = TimeSpan.FromTicks(RetryBaseDelay.Ticks * (long)Math.Pow(2, AttemptCount - 1));
        NextAttemptAt = now.Add(delay);
    }

    /// <summary>
    /// Forces dead-letter without scheduling further retries.
    /// </summary>
    public void MarkDeadLetter(string error, DateTimeOffset startedAt, DateTimeOffset finishedAt)
    {
        AttemptCount++;
        LastError = error;
        Status = MessageJobStatus.DeadLetter;
        _attemptLogs.Add(JobAttemptLog.Create(Id, AttemptCount, AttemptOutcome.Failed, error, startedAt, finishedAt));
    }

    /// <summary>
    /// Whether the job is eligible for processing at the given instant.
    /// </summary>
    public bool IsDue(DateTimeOffset now) =>
        Status is MessageJobStatus.Pending or MessageJobStatus.Failed && NextAttemptAt <= now;
}
