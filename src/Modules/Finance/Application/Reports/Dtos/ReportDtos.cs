namespace Finance.Application.Reports.Dtos;

/// <summary>API DTO for one day in the cash-flow report.</summary>
public sealed class CashFlowDayDto
{
    public DateOnly Date { get; init; }
    public decimal RealizedInflow { get; init; }
    public decimal RealizedOutflow { get; init; }
    public decimal NetRealized { get; init; }
    public decimal ExpectedReceivable { get; init; }
    public decimal ExpectedPayable { get; init; }
}

/// <summary>API DTO for cash-basis AP/AR movement.</summary>
public sealed class CashFlowReportDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<CashFlowDayDto> Days { get; init; } = Array.Empty<CashFlowDayDto>();
    public decimal TotalRealizedInflow { get; init; }
    public decimal TotalRealizedOutflow { get; init; }
    public decimal TotalExpectedReceivable { get; init; }
    public decimal TotalExpectedPayable { get; init; }
}

/// <summary>API DTO for one DRE category line.</summary>
public sealed class SimplifiedDreLineDto
{
    public Guid CategoryId { get; init; }
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public decimal Expense { get; init; }
}

/// <summary>API DTO for simplified monthly income statement.</summary>
public sealed class SimplifiedDreReportDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public IReadOnlyList<SimplifiedDreLineDto> Lines { get; init; } = Array.Empty<SimplifiedDreLineDto>();
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetResult { get; init; }
}
