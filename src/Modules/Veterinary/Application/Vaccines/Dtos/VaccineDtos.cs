using Core.Domain.Entities;

namespace Veterinary.Application.Vaccines.Dtos;

/// <summary>Alert classification for clinic backoffice lists.</summary>
public enum VaccineAlertStatusDto
{
    Overdue = 1,
    Upcoming = 2
}

/// <summary>Input line when defining protocol doses.</summary>
public sealed record VaccineProtocolDoseInput(
    Guid? DoseId,
    string Label,
    int MinAgeInDays,
    int? MaxAgeInDays,
    int? IntervalFromPreviousInDays,
    int? NextDoseIntervalInDays);

/// <summary>Protocol catalog list item.</summary>
public sealed class VaccineProtocolDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public PetSpecies Species { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<VaccineProtocolDoseDto> Doses { get; init; } = Array.Empty<VaccineProtocolDoseDto>();
}

/// <summary>Protocol dose definition.</summary>
public sealed class VaccineProtocolDoseDto
{
    public Guid Id { get; init; }
    public int Sequence { get; init; }
    public string Label { get; init; } = string.Empty;
    public int MinAgeInDays { get; init; }
    public int? MaxAgeInDays { get; init; }
    public int? IntervalFromPreviousInDays { get; init; }
    public int? NextDoseIntervalInDays { get; init; }
}

/// <summary>Applied vaccine row.</summary>
public sealed class VaccineDoseDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
    public Guid? ProtocolId { get; init; }
    public Guid? ProtocolDoseId { get; init; }
}

/// <summary>Suggested protocol step not yet applied.</summary>
public sealed class SuggestedVaccineDoseDto
{
    public Guid ProtocolId { get; init; }
    public string ProtocolName { get; init; } = string.Empty;
    public Guid ProtocolDoseId { get; init; }
    public string Label { get; init; } = string.Empty;
    public int Sequence { get; init; }
}

/// <summary>Digital vaccination card for export/print.</summary>
public sealed class VaccinationCardDto
{
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public PetSpecies Species { get; init; }
    public string Breed { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string TutorName { get; init; } = string.Empty;
    public IReadOnlyList<VaccineDoseDto> AppliedDoses { get; init; } = Array.Empty<VaccineDoseDto>();
    public IReadOnlyList<SuggestedVaccineDoseDto> SuggestedDoses { get; init; } = Array.Empty<SuggestedVaccineDoseDto>();
}

/// <summary>Clinic alert row (Automations feed contract).</summary>
public sealed class VaccineAlertDto
{
    public Guid VaccineDoseId { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public string VaccineName { get; init; } = string.Empty;
    public DateTimeOffset NextDueDate { get; init; }
    public VaccineAlertStatusDto Status { get; init; }
}
