namespace TutorPortal.Application.Scheduling.Dtos;

/// <summary>Bookable service row for tutor scheduling wizard.</summary>
public sealed class TutorBookableServiceDto
{
    /// <summary>Service id (sentinel for clinical consultation).</summary>
    public Guid ServiceId { get; init; }

    /// <summary>Human-readable service name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Clinical or grooming agenda.</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>Expected appointment duration in minutes.</summary>
    public int DurationInMinutes { get; init; }
}

/// <summary>Professional with availability on a given day.</summary>
public sealed class TutorBookingProfessionalDto
{
    /// <summary>Staff user id (veterinarian or groomer).</summary>
    public Guid ProfessionalId { get; init; }

    /// <summary>Display name for the tutor UI.</summary>
    public string DisplayName { get; init; } = string.Empty;
}

/// <summary>Start instant the tutor can book.</summary>
public sealed class TutorAvailableSlotDto
{
    /// <summary>Appointment start date/time (UTC offset preserved).</summary>
    public DateTimeOffset Start { get; init; }

    /// <summary>Duration in minutes for this booking.</summary>
    public int DurationInMinutes { get; init; }
}

/// <summary>Upcoming tutor-owned appointment summary.</summary>
public sealed class TutorPetAppointmentDto
{
    /// <summary>Appointment aggregate id.</summary>
    public Guid Id { get; init; }

    /// <summary>Clinical or grooming.</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>Scheduled start.</summary>
    public DateTimeOffset Date { get; init; }

    /// <summary>Current status name.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Service or reason label.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Whether the tutor may cancel from the portal.</summary>
    public bool CanCancel { get; init; }
}
