namespace Finance.Domain.Models;

/// <summary>
/// Cash-basis movement for a single calendar day within a statement period.
/// </summary>
public sealed record CashFlowDayLine(
    DateOnly Date,
    decimal RealizedInflow,
    decimal RealizedOutflow,
    decimal ExpectedReceivable,
    decimal ExpectedPayable)
{
    /// <summary>Net realized cash movement (inflows minus outflows).</summary>
    public decimal NetRealized => RealizedInflow - RealizedOutflow;
}

/// <summary>
/// Cash-basis statement aggregating AP/AR settlements by day.
/// </summary>
public sealed record CashFlowStatement(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<CashFlowDayLine> Days,
    decimal TotalRealizedInflow,
    decimal TotalRealizedOutflow,
    decimal TotalExpectedReceivable,
    decimal TotalExpectedPayable);

/// <summary>
/// One DRE line grouped by financial category for the competence month.
/// </summary>
public sealed record IncomeStatementLine(
    Guid CategoryId,
    string CategoryCode,
    string CategoryName,
    decimal Revenue,
    decimal Expense);

/// <summary>
/// Simplified income statement (operational competence by issue date on titles).
/// </summary>
public sealed record SimplifiedIncomeStatement(
    int Year,
    int Month,
    IReadOnlyList<IncomeStatementLine> Lines,
    decimal TotalRevenue,
    decimal TotalExpense)
{
    /// <summary>Revenue minus expense for the period.</summary>
    public decimal NetResult => TotalRevenue - TotalExpense;
}
