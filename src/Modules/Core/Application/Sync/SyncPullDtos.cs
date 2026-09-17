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
public sealed class PullChangesResult
{
    public IReadOnlyList<SyncTutorDto> Tutors { get; init; } = Array.Empty<SyncTutorDto>();
    public IReadOnlyList<SyncPetDto> Pets { get; init; } = Array.Empty<SyncPetDto>();
    public DateTimeOffset NextSince { get; init; }
    public bool HasMore { get; init; }
}
