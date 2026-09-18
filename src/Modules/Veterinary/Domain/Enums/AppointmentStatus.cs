namespace Veterinary.Domain.Enums;

/// <summary>
/// Lifecycle states for a clinical appointment in the unified agenda.
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5,
    InProgress = 6
}
