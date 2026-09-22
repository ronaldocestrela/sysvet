using Automations.Application.Reminders;

namespace Automations.Application.Abstractions;

/// <summary>
/// Collects reminder candidates for the current tenant and local calendar day.
/// </summary>
public interface IReminderCandidateSource
{
    /// <summary>
    /// Returns zero or more candidates that should fire on this scan for <paramref name="localToday"/>.
    /// </summary>
    Task<IReadOnlyList<ReminderCandidate>> CollectAsync(DateOnly localToday, CancellationToken cancellationToken);
}
