using Finance.Application.Reconciliation.Dtos;
using Finance.Domain.Entities;
using Finance.Domain.Enums;

namespace Finance.Application.Reconciliation;

internal static class CardReconciliationMapper
{
    public static CardReconciliationBatchSummaryDto ToSummary(CardReconciliationBatch batch) =>
        new()
        {
            Id = batch.Id,
            Reference = batch.Reference,
            PeriodFrom = batch.PeriodFrom,
            PeriodTo = batch.PeriodTo,
            ImportedAt = batch.ImportedAt,
            MatchedCount = batch.Lines.Count(l => l.Status == CardReconciliationLineStatus.Matched),
            UnmatchedCount = batch.Lines.Count(l => l.Status == CardReconciliationLineStatus.Unmatched),
            DivergentCount = batch.Lines.Count(l => l.Status == CardReconciliationLineStatus.Divergent)
        };

    public static CardReconciliationBatchDetailDto ToDetail(CardReconciliationBatch batch) =>
        new()
        {
            Id = batch.Id,
            Reference = batch.Reference,
            PeriodFrom = batch.PeriodFrom,
            PeriodTo = batch.PeriodTo,
            ImportedAt = batch.ImportedAt,
            Lines = batch.Lines.Select(l => new CardReconciliationLineDto
            {
                Id = l.Id,
                Nsu = l.Nsu,
                Amount = l.Amount,
                Method = l.Method,
                Status = l.Status,
                MatchedAllocationId = l.MatchedAllocationId
            }).ToList()
        };
}
