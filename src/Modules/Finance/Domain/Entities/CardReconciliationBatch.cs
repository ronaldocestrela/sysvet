using Core.Domain;
using Finance.Domain.Enums;
using Finance.Domain.Models;

namespace Finance.Domain.Entities;

/// <summary>
/// Batch import of acquirer card settlements matched against AR allocations (TEF PoC).
/// </summary>
public sealed class CardReconciliationBatch : AggregateRoot
{
    public string Reference { get; private set; } = string.Empty;
    public DateOnly PeriodFrom { get; private set; }
    public DateOnly PeriodTo { get; private set; }
    public DateTimeOffset ImportedAt { get; private set; }

    private readonly List<CardReconciliationLine> _lines = new();

    /// <summary>Statement lines in this batch.</summary>
    public IReadOnlyCollection<CardReconciliationLine> Lines => _lines.AsReadOnly();

    private CardReconciliationBatch() { }

    /// <summary>Creates a batch from imported statement lines and runs NSU matching.</summary>
    public static Result<CardReconciliationBatch> Import(
        string reference,
        DateOnly periodFrom,
        DateOnly periodTo,
        IReadOnlyList<CardStatementImportLine> statementLines,
        IReadOnlyList<CardSettlementSnapshot> settlements,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return Result.Failure<CardReconciliationBatch>(ErrorCodes.Reconciliation.InvalidReference);
        }

        if (statementLines.Count == 0)
        {
            return Result.Failure<CardReconciliationBatch>(ErrorCodes.Reconciliation.EmptyStatement);
        }

        var batchId = id ?? Guid.NewGuid();
        var batch = new CardReconciliationBatch
        {
            Id = batchId,
            Reference = reference.Trim(),
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            ImportedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in statementLines)
        {
            if (string.IsNullOrWhiteSpace(line.Nsu) || line.Amount <= 0)
            {
                return Result.Failure<CardReconciliationBatch>(ErrorCodes.Reconciliation.InvalidLine);
            }

            batch._lines.Add(CardReconciliationLine.Create(
                batchId,
                line.Nsu,
                line.Amount,
                line.Method,
                line.Fee,
                line.OccurredAt));
        }

        batch.MatchLines(settlements);
        return Result.Success(batch);
    }

    /// <summary>Matches lines by NSU and amount against settlement snapshots.</summary>
    public void MatchLines(IReadOnlyList<CardSettlementSnapshot> settlements)
    {
        var byNsu = settlements
            .Where(s => !string.IsNullOrWhiteSpace(s.Nsu))
            .GroupBy(s => s.Nsu.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var line in _lines)
        {
            if (!byNsu.TryGetValue(line.Nsu, out var candidates))
            {
                line.ApplyMatch(null, CardReconciliationLineStatus.Unmatched);
                continue;
            }

            var exact = candidates.FirstOrDefault(c => c.Amount == line.Amount);
            if (exact is not null)
            {
                line.ApplyMatch(exact.AllocationId, CardReconciliationLineStatus.Matched);
            }
            else
            {
                line.ApplyMatch(null, CardReconciliationLineStatus.Divergent);
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>Rehydrates from persistence.</summary>
    public static CardReconciliationBatch Restore(
        Guid id,
        string reference,
        DateOnly periodFrom,
        DateOnly periodTo,
        DateTimeOffset importedAt,
        DateTimeOffset updatedAt,
        IEnumerable<CardReconciliationLine> lines)
    {
        var batch = new CardReconciliationBatch
        {
            Id = id,
            Reference = reference,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            ImportedAt = importedAt,
            UpdatedAt = updatedAt
        };
        batch._lines.AddRange(lines);
        return batch;
    }
}
