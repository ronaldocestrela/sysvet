using Core.Domain;
using Finance.Domain.Enums;

namespace Finance.Domain.Entities;

/// <summary>
/// Imported acquirer statement line within a reconciliation batch.
/// </summary>
public sealed class CardReconciliationLine : Entity
{
    public Guid BatchId { get; private set; }
    public string Nsu { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string? Method { get; private set; }
    public decimal? Fee { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public CardReconciliationLineStatus Status { get; private set; }
    public Guid? MatchedAllocationId { get; private set; }

    private CardReconciliationLine() { }

    internal static CardReconciliationLine Create(
        Guid batchId,
        string nsu,
        decimal amount,
        string? method,
        decimal? fee,
        DateTimeOffset occurredAt,
        Guid? id = null)
    {
        return new CardReconciliationLine
        {
            Id = id ?? Guid.NewGuid(),
            BatchId = batchId,
            Nsu = nsu.Trim(),
            Amount = amount,
            Method = string.IsNullOrWhiteSpace(method) ? null : method.Trim(),
            Fee = fee,
            OccurredAt = occurredAt,
            Status = CardReconciliationLineStatus.Unmatched
        };
    }

    internal void ApplyMatch(Guid? allocationId, CardReconciliationLineStatus status)
    {
        Status = status;
        MatchedAllocationId = allocationId;
    }

    /// <summary>Rehydrates from persistence.</summary>
    public static CardReconciliationLine Restore(
        Guid id,
        Guid batchId,
        string nsu,
        decimal amount,
        string? method,
        decimal? fee,
        DateTimeOffset occurredAt,
        CardReconciliationLineStatus status,
        Guid? matchedAllocationId,
        DateTimeOffset updatedAt)
    {
        return new CardReconciliationLine
        {
            Id = id,
            BatchId = batchId,
            Nsu = nsu,
            Amount = amount,
            Method = method,
            Fee = fee,
            OccurredAt = occurredAt,
            Status = status,
            MatchedAllocationId = matchedAllocationId,
            UpdatedAt = updatedAt
        };
    }
}
