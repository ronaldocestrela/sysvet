using Finance.Domain.Enums;

namespace Finance.Application.Reconciliation.Dtos;

/// <summary>Summary of an imported reconciliation batch.</summary>
public sealed class CardReconciliationBatchSummaryDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public DateTimeOffset ImportedAt { get; init; }
    public int MatchedCount { get; init; }
    public int UnmatchedCount { get; init; }
    public int DivergentCount { get; init; }
}

/// <summary>Batch detail with statement lines.</summary>
public sealed class CardReconciliationBatchDetailDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public DateOnly PeriodFrom { get; init; }
    public DateOnly PeriodTo { get; init; }
    public DateTimeOffset ImportedAt { get; init; }
    public IReadOnlyList<CardReconciliationLineDto> Lines { get; init; } = Array.Empty<CardReconciliationLineDto>();
}

public sealed class CardReconciliationLineDto
{
    public Guid Id { get; init; }
    public string Nsu { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? Method { get; init; }
    public CardReconciliationLineStatus Status { get; init; }
    public Guid? MatchedAllocationId { get; init; }
}

/// <summary>Card settlement from AR not yet matched in any batch.</summary>
public sealed class UnmatchedCardSettlementDto
{
    public Guid AllocationId { get; init; }
    public string Nsu { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty;
}

public sealed class ImportCardStatementLineRequest
{
    public string Nsu { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public decimal? Fee { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}
