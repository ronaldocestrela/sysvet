namespace Clients.Infrastructure.Sync;

/// <summary>
/// Client-side mirror of API sync HTTP contracts.
/// </summary>
public sealed class ClientSyncPushResult
{
    public IReadOnlyList<Guid> ProcessedIds { get; init; } = Array.Empty<Guid>();
    public Guid? FailedMessageId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsPermanentFailure { get; init; }
}

/// <summary>
/// Pull page returned by the sync API.
/// </summary>
public sealed class ClientPullChangesResult
{
    public IReadOnlyList<ClientSyncTutorDto> Tutors { get; init; } = Array.Empty<ClientSyncTutorDto>();
    public IReadOnlyList<ClientSyncPetDto> Pets { get; init; } = Array.Empty<ClientSyncPetDto>();
    public IReadOnlyList<ClientSyncAppointmentDto> Appointments { get; init; } = Array.Empty<ClientSyncAppointmentDto>();
    public IReadOnlyList<ClientSyncScheduleSlotDto> ScheduleSlots { get; init; } = Array.Empty<ClientSyncScheduleSlotDto>();
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}

/// <summary>Tutor row from sync pull.</summary>
public sealed class ClientSyncTutorDto
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

/// <summary>Pet row from sync pull.</summary>
public sealed class ClientSyncPetDto
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

/// <summary>Appointment row from sync pull.</summary>
public sealed class ClientSyncAppointmentDto
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

/// <summary>Schedule slot row from sync pull.</summary>
public sealed class ClientSyncScheduleSlotDto
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
