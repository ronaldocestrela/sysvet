namespace Automations.Domain.Enums;

/// <summary>
/// Lifecycle state of a queued outbound message job.
/// </summary>
public enum MessageJobStatus
{
    Pending = 1,
    Processing = 2,
    Succeeded = 3,
    Failed = 4,
    DeadLetter = 5
}
