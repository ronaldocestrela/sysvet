namespace Petshop.Domain.Enums;

/// <summary>
/// Lifecycle states for a grooming salon appointment.
/// </summary>
public enum GroomingAppointmentStatus
{
    Scheduled = 1,
    Confirmed = 2,
    Completed = 3,
    Cancelled = 4,
    NoShow = 5,
    InProgress = 6
}
