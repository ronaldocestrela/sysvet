using Core.Application.Tutors.Queries;

namespace Core.Application.Privacy;

/// <summary>
/// Portable export package for a tutor data-subject request.
/// </summary>
public sealed class TutorPersonalDataExportDto
{
    /// <summary>Exported tutor id.</summary>
    public Guid TutorId { get; init; }

    /// <summary>UTC export timestamp.</summary>
    public DateTimeOffset ExportedAt { get; init; }

    /// <summary>CRM tutor fields.</summary>
    public TutorDto Tutor { get; init; } = null!;

    /// <summary>Linked pets (names only; not tutor PII).</summary>
    public IReadOnlyList<PrivacyPetDto> Pets { get; init; } = Array.Empty<PrivacyPetDto>();

    /// <summary>Additional slices keyed by module then logical name.</summary>
    public Dictionary<string, Dictionary<string, object?>> Modules { get; init; } = new();
}

/// <summary>Minimal pet projection for privacy export.</summary>
public sealed class PrivacyPetDto
{
    /// <summary>Pet id.</summary>
    public Guid Id { get; init; }

    /// <summary>Pet name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Species label.</summary>
    public string Species { get; init; } = string.Empty;
}
