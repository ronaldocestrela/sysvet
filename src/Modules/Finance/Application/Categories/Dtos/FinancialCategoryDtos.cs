using Finance.Domain.Enums;

namespace Finance.Application.Categories.Dtos;

/// <summary>Financial category DTO.</summary>
public sealed class FinancialCategoryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CategoryDirection Direction { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
}
