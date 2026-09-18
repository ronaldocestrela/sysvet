namespace Veterinary.Application.Hospitalizations.Dtos;

/// <summary>Active hospitalization list item.</summary>
public sealed class HospitalizationListItemDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>Administration slot on the execution map.</summary>
public sealed class ExecutionMapAdministrationDto
{
    public Guid Id { get; init; }
    public Guid MedicationOrderId { get; init; }
    public string MedicationName { get; init; } = string.Empty;
    public string Dose { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>Occupied bed on the execution map.</summary>
public sealed class ExecutionMapOccupancyDto
{
    public Guid HospitalizationId { get; init; }
    public Guid PetId { get; init; }
    public string PetName { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public IReadOnlyList<ExecutionMapAdministrationDto> Administrations { get; init; } = Array.Empty<ExecutionMapAdministrationDto>();
}

/// <summary>Bed cell on the execution map.</summary>
public sealed class ExecutionMapBedDto
{
    public Guid BedId { get; init; }
    public string BedCode { get; init; } = string.Empty;
    public ExecutionMapOccupancyDto? Occupancy { get; init; }
}

/// <summary>Ward row on the execution map.</summary>
public sealed class ExecutionMapWardDto
{
    public Guid WardUnitId { get; init; }
    public string WardName { get; init; } = string.Empty;
    public IReadOnlyList<ExecutionMapBedDto> Beds { get; init; } = Array.Empty<ExecutionMapBedDto>();
}

/// <summary>Full execution map for a UTC day.</summary>
public sealed class ExecutionMapDto
{
    public DateOnly Date { get; init; }
    public IReadOnlyList<ExecutionMapWardDto> Wards { get; init; } = Array.Empty<ExecutionMapWardDto>();
}

/// <summary>Hospitalization detail for clinical panel.</summary>
public sealed class HospitalizationDetailDto
{
    public Guid Id { get; init; }
    public Guid PetId { get; init; }
    public Guid VeterinarianId { get; init; }
    public Guid BedId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTimeOffset AdmittedAt { get; init; }
    public DateTimeOffset? DischargedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
