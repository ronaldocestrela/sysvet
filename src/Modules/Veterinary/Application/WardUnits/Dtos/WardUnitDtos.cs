namespace Veterinary.Application.WardUnits.Dtos;

/// <summary>Ward unit list item.</summary>
public sealed class WardUnitListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int BedCount { get; init; }
}

/// <summary>Bed line in ward detail.</summary>
public sealed class BedDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>Ward unit with beds.</summary>
public sealed class WardUnitDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<BedDto> Beds { get; init; } = Array.Empty<BedDto>();
}
