using Core.Domain;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;

namespace Finance.Domain.Services;

/// <summary>
/// Builds cash-flow and simplified DRE from AP/AR titles without mixing Sales register movements.
/// </summary>
public static class FinancialStatementCalculator
{
    private const int MaxCashFlowDays = 366;

    /// <summary>
    /// Aggregates realized settlements by <c>PaidAt</c> and expected open balances by <c>DueDate</c>.
    /// </summary>
    public static Result<CashFlowStatement> BuildCashFlow(
        DateOnly from,
        DateOnly to,
        IReadOnlyList<FinancialTitle> titles)
    {
        var range = ValidateDateRange(from, to);
        if (range.IsFailure)
        {
            return Result.Failure<CashFlowStatement>(range.Error);
        }

        var dayCount = to.DayNumber - from.DayNumber + 1;
        if (dayCount > MaxCashFlowDays)
        {
            return Result.Failure<CashFlowStatement>(ErrorCodes.Report.RangeTooLarge);
        }

        var buckets = new Dictionary<DateOnly, (decimal In, decimal Out, decimal ExpRec, decimal ExpPay)>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            buckets[d] = (0, 0, 0, 0);
        }

        foreach (var title in titles.Where(t => t.Status != TitleStatus.Cancelled))
        {
            foreach (var allocation in title.Allocations)
            {
                var paidDate = DateOnly.FromDateTime(allocation.PaidAt.UtcDateTime);
                if (paidDate < from || paidDate > to)
                {
                    continue;
                }

                if (!buckets.TryGetValue(paidDate, out var bucket))
                {
                    continue;
                }

                var amount = allocation.Amount;
                if (title.Direction == TitleDirection.Receivable)
                {
                    if (allocation.Kind == AllocationKind.Settlement)
                    {
                        bucket.In += amount;
                    }
                    else
                    {
                        bucket.In -= amount;
                    }
                }
                else if (allocation.Kind == AllocationKind.Settlement)
                {
                    bucket.Out += amount;
                }
                else
                {
                    bucket.Out -= amount;
                }

                buckets[paidDate] = bucket;
            }

            if (title.DueDate >= from && title.DueDate <= to && title.OpenAmount > 0)
            {
                var due = title.DueDate;
                var b = buckets[due];
                if (title.Direction == TitleDirection.Receivable)
                {
                    b.ExpRec += title.OpenAmount;
                }
                else
                {
                    b.ExpPay += title.OpenAmount;
                }

                buckets[due] = b;
            }
        }

        var days = buckets
            .OrderBy(k => k.Key)
            .Select(k => new CashFlowDayLine(k.Key, k.Value.In, k.Value.Out, k.Value.ExpRec, k.Value.ExpPay))
            .ToList();

        return Result.Success(new CashFlowStatement(
            from,
            to,
            days,
            days.Sum(d => d.RealizedInflow),
            days.Sum(d => d.RealizedOutflow),
            days.Sum(d => d.ExpectedReceivable),
            days.Sum(d => d.ExpectedPayable)));
    }

    /// <summary>
    /// Groups revenue and expense by category using title issue date (competence) and month reversals.
    /// </summary>
    public static Result<SimplifiedIncomeStatement> BuildSimplifiedDre(
        int year,
        int month,
        IReadOnlyList<FinancialTitle> titles,
        IReadOnlyDictionary<Guid, FinancialCategory> categories)
    {
        if (month is < 1 or > 12)
        {
            return Result.Failure<SimplifiedIncomeStatement>(ErrorCodes.Report.InvalidMonth);
        }

        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var revenueByCategory = new Dictionary<Guid, decimal>();
        var expenseByCategory = new Dictionary<Guid, decimal>();

        foreach (var title in titles.Where(t => t.Status != TitleStatus.Cancelled))
        {
            if (title.IssueDate >= from && title.IssueDate <= to)
            {
                if (title.Direction == TitleDirection.Receivable)
                {
                    revenueByCategory[title.CategoryId] = revenueByCategory.GetValueOrDefault(title.CategoryId) + title.OriginalAmount;
                }
                else
                {
                    expenseByCategory[title.CategoryId] = expenseByCategory.GetValueOrDefault(title.CategoryId) + title.OriginalAmount;
                }
            }

            foreach (var allocation in title.Allocations.Where(a => a.Kind == AllocationKind.Reversal))
            {
                var paidDate = DateOnly.FromDateTime(allocation.PaidAt.UtcDateTime);
                if (paidDate < from || paidDate > to)
                {
                    continue;
                }

                if (title.Direction == TitleDirection.Receivable)
                {
                    revenueByCategory[title.CategoryId] = revenueByCategory.GetValueOrDefault(title.CategoryId) - allocation.Amount;
                }
                else
                {
                    expenseByCategory[title.CategoryId] = expenseByCategory.GetValueOrDefault(title.CategoryId) - allocation.Amount;
                }
            }
        }

        var categoryIds = revenueByCategory.Keys.Union(expenseByCategory.Keys).Distinct();
        var lines = new List<IncomeStatementLine>();

        foreach (var categoryId in categoryIds.OrderBy(id => categories.TryGetValue(id, out var c) ? c.Code : id.ToString()))
        {
            categories.TryGetValue(categoryId, out var category);
            var revenue = revenueByCategory.GetValueOrDefault(categoryId);
            var expense = expenseByCategory.GetValueOrDefault(categoryId);
            if (revenue == 0 && expense == 0)
            {
                continue;
            }

            lines.Add(new IncomeStatementLine(
                categoryId,
                category?.Code ?? categoryId.ToString("N"),
                category?.Name ?? "Unknown",
                revenue,
                expense));
        }

        return Result.Success(new SimplifiedIncomeStatement(
            year,
            month,
            lines,
            lines.Sum(l => l.Revenue),
            lines.Sum(l => l.Expense)));
    }

    /// <summary>Validates export covers exactly one calendar month.</summary>
    public static Result ValidateFullCalendarMonth(DateOnly from, DateOnly to)
    {
        var range = ValidateDateRange(from, to);
        if (range.IsFailure)
        {
            return range;
        }

        if (from.Day != 1 || to != from.AddMonths(1).AddDays(-1))
        {
            return Result.Failure(ErrorCodes.Report.ExportNotFullMonth);
        }

        return Result.Success();
    }

    private static Result ValidateDateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
        {
            return Result.Failure(ErrorCodes.Report.InvalidDateRange);
        }

        return Result.Success();
    }
}
