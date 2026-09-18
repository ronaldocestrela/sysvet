namespace Core.Application.Sync;

/// <summary>
/// Tutor row returned by sync pull (includes soft-deleted tombstones).
/// </summary>
public sealed class SyncTutorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>
/// Pet row returned by sync pull (includes soft-deleted tombstones).
/// </summary>
public sealed class SyncPetDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string Breed { get; init; } = string.Empty;
    public string Sex { get; init; } = string.Empty;
    public Guid TutorId { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>
/// Paginated pull page for client upsert.
/// </summary>
/// <summary>Appointment row returned by sync pull.</summary>
public sealed class SyncAppointmentDto
{
    public Guid Id { get; init; }
    public Guid TutorId { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public int DurationInMinutes { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

/// <summary>Schedule slot row returned by sync pull.</summary>
public sealed class SyncScheduleSlotDto
{
    public Guid Id { get; init; }
    public Guid VeterinarianId { get; init; }
    public DateTimeOffset Date { get; init; }
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsAvailable { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public string RowVersion { get; init; } = string.Empty;
}

public sealed class PullChangesResult
{
    public IReadOnlyList<SyncTutorDto> Tutors { get; init; } = Array.Empty<SyncTutorDto>();
    public IReadOnlyList<SyncPetDto> Pets { get; init; } = Array.Empty<SyncPetDto>();
    public IReadOnlyList<SyncAppointmentDto> Appointments { get; init; } = Array.Empty<SyncAppointmentDto>();
    public IReadOnlyList<SyncScheduleSlotDto> ScheduleSlots { get; init; } = Array.Empty<SyncScheduleSlotDto>();
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}
