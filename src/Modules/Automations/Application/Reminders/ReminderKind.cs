namespace Automations.Application.Reminders;

/// <summary>
/// Business trigger that produced a reminder candidate.
/// </summary>
public enum ReminderKind
{
    Vaccine = 1,
    Appointment = 2,
    Birthday = 3,
    FollowUp = 4
}
