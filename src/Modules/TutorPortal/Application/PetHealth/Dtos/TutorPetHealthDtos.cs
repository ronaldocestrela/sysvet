using Core.Domain.Entities;

namespace TutorPortal.Application.PetHealth.Dtos;

/// <summary>Applied vaccine row exposed to the tutor portal.</summary>
public sealed class TutorVaccineDoseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
}

/// <summary>Protocol suggestion not yet applied.</summary>
public sealed class TutorSuggestedVaccineDoseDto
{
    public string ProtocolName { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Sequence { get; init; }
}

/// <summary>Digital vaccination card for tutor read-only view.</summary>
public sealed class TutorVaccinationCardDto
{
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public PetSpecies Species { get; init; }
    public string Breed { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string TutorName { get; init; } = string.Empty;
    public IReadOnlyList<TutorVaccineDoseDto> AppliedDoses { get; init; } = Array.Empty<TutorVaccineDoseDto>();
    public IReadOnlyList<TutorSuggestedVaccineDoseDto> SuggestedDoses { get; init; } = Array.Empty<TutorSuggestedVaccineDoseDto>();
}

/// <summary>Clinical exam summary for the tutor portal.</summary>
public sealed class TutorPetExamDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string ResultSummary { get; init; } = string.Empty;
}

/// <summary>Sanitized timeline row (no clinical notes or diagnosis).</summary>
public sealed class TutorPetTimelineItemDto
{
    public Guid Id { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}
