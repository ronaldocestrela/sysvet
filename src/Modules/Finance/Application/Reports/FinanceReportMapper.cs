using Finance.Application.Reports.Dtos;
using Finance.Domain.Models;

namespace Finance.Application.Reports;

/// <summary>
/// Maps domain statement models to API/client DTOs (shared by handlers and offline clients).
/// </summary>
public static class FinanceReportMapper
{
    /// <summary>Maps cash-flow domain model to DTO.</summary>
    public static CashFlowReportDto ToDto(CashFlowStatement statement) =>
        new()
        {
            From = statement.From,
            To = statement.To,
            Days = statement.Days.Select(d => new CashFlowDayDto
            {
                Date = d.Date,
                RealizedInflow = d.RealizedInflow,
                RealizedOutflow = d.RealizedOutflow,
                NetRealized = d.NetRealized,
                ExpectedReceivable = d.ExpectedReceivable,
                ExpectedPayable = d.ExpectedPayable
            }).ToList(),
            TotalRealizedInflow = statement.TotalRealizedInflow,
            TotalRealizedOutflow = statement.TotalRealizedOutflow,
            TotalExpectedReceivable = statement.TotalExpectedReceivable,
            TotalExpectedPayable = statement.TotalExpectedPayable
        };

    /// <summary>Maps simplified DRE domain model to DTO.</summary>
    public static SimplifiedDreReportDto ToDto(SimplifiedIncomeStatement statement) =>
        new()
        {
            Year = statement.Year,
            Month = statement.Month,
            Lines = statement.Lines.Select(l => new SimplifiedDreLineDto
            {
                CategoryId = l.CategoryId,
                CategoryCode = l.CategoryCode,
                CategoryName = l.CategoryName,
                Revenue = l.Revenue,
                Expense = l.Expense
            }).ToList(),
            TotalRevenue = statement.TotalRevenue,
            TotalExpense = statement.TotalExpense,
            NetResult = statement.NetResult
        };
}
