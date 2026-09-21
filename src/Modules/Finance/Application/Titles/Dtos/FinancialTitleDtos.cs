using Finance.Domain.Enums;

namespace Finance.Application.Titles.Dtos;

/// <summary>Financial title list/detail DTO.</summary>
public sealed class FinancialTitleDto
{
    public Guid Id { get; init; }
    public TitleDirection Direction { get; init; }
    public TitleStatus Status { get; init; }
    public TitleSourceType SourceType { get; init; }
    public Guid? SourceId { get; init; }
    public string SourceInstallmentKey { get; init; } = string.Empty;
    public PartyKind PartyKind { get; init; }
    public Guid? PartyId { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? CostCenterId { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly DueDate { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal SettledAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<TitleAllocationDto> Allocations { get; init; } = Array.Empty<TitleAllocationDto>();
}

/// <summary>Allocation line on a title.</summary>
public sealed class TitleAllocationDto
{
    public Guid Id { get; init; }
    public decimal Amount { get; init; }
    public DateTimeOffset PaidAt { get; init; }
    public string Method { get; init; } = string.Empty;
    public AllocationKind Kind { get; init; }
}
