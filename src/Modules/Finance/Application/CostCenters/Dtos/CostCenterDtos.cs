namespace Finance.Application.CostCenters.Dtos;

/// <summary>Cost center DTO.</summary>
public sealed class CostCenterDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
