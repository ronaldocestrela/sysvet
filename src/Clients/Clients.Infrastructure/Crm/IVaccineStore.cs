using Core.Domain;

namespace Clients.Infrastructure.Crm;

/// <summary>Offline-first vaccination card, doses, protocols, and clinic alerts.</summary>
public interface IVaccineStore
{
    Task<Result<IReadOnlyList<VaccineDoseListItemDto>>> GetDosesByPetAsync(Guid petId, CancellationToken cancellationToken = default);

    Task<Result<VaccinationCardDto>> GetCardAsync(Guid petId, CancellationToken cancellationToken = default);

    Task<Result<Guid>> RegisterDoseAsync(RegisterVaccineDoseRequest request, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<VaccineAlertListItemDto>>> GetAlertsAsync(VaccineAlertFilter filter, CancellationToken cancellationToken = default);

    Task<Result<int>> CountOverdueAlertsAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<VaccineProtocolListItemDto>>> ListProtocolsAsync(CancellationToken cancellationToken = default);
}

public sealed class RegisterVaccineDoseRequest
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
    public Guid? ProtocolDoseId { get; init; }
}

public sealed class VaccineAlertFilter
{
    public string? Status { get; init; }
    public int HorizonDays { get; init; } = 7;
}

public sealed class VaccineDoseListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string BatchNumber { get; init; } = string.Empty;
    public DateTimeOffset AppliedAt { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
}

public sealed class VaccinationCardDto
{
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
    public string Breed { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string TutorName { get; init; } = string.Empty;
    public IReadOnlyList<VaccineDoseListItemDto> AppliedDoses { get; init; } = Array.Empty<VaccineDoseListItemDto>();
    public IReadOnlyList<SuggestedVaccineDoseListItemDto> SuggestedDoses { get; init; } = Array.Empty<SuggestedVaccineDoseListItemDto>();
}

public sealed class SuggestedVaccineDoseListItemDto
{
    public Guid ProtocolId { get; init; }
    public string ProtocolName { get; init; } = string.Empty;
    public Guid ProtocolDoseId { get; init; }
    public string Label { get; init; } = string.Empty;
}

public sealed class VaccineAlertListItemDto
{
    public Guid VaccineDoseId { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public string VaccineName { get; init; } = string.Empty;
    public DateTimeOffset NextDueDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class VaccineProtocolListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Species { get; init; } = string.Empty;
}
