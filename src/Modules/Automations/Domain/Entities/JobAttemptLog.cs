using Automations.Domain.Enums;
using Core.Domain;

namespace Automations.Domain.Entities;

/// <summary>
/// Append-only record of one delivery attempt for a <see cref="MessageJob"/>.
/// </summary>
public sealed class JobAttemptLog : Entity
{
    public Guid MessageJobId { get; private set; }
    public int AttemptNumber { get; private set; }
    public AttemptOutcome Outcome { get; private set; }
    public string? Detail { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset FinishedAt { get; private set; }

    private JobAttemptLog() { }

    internal static JobAttemptLog Create(
        Guid messageJobId,
        int attemptNumber,
        AttemptOutcome outcome,
        string? detail,
        DateTimeOffset startedAt,
        DateTimeOffset finishedAt)
    {
        return new JobAttemptLog
        {
            MessageJobId = messageJobId,
            AttemptNumber = attemptNumber,
            Outcome = outcome,
            Detail = detail,
            StartedAt = startedAt,
            FinishedAt = finishedAt
        };
    }
}
