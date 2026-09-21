using Core.Domain;

namespace Clients.Infrastructure.Finance;

/// <summary>
/// Client port for AP/AR titles — offline mirror with sync outbox for manual operations.
/// </summary>
public interface IFinanceStore
{
    Task<Result<IReadOnlyList<FinanceTitleListItemDto>>> ListTitlesAsync(CancellationToken cancellationToken = default);

    Task<Result<BalanceProjectionClientDto>> GetProjectionAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<Result> SettleTitleAsync(Guid titleId, decimal amount, string method, CancellationToken cancellationToken = default);

    Task<Result<CashFlowReportClientDto>> GetCashFlowAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<Result<SimplifiedDreReportClientDto>> GetSimplifiedDreAsync(int year, int month, CancellationToken cancellationToken = default);

    Task<Result<byte[]>> ExportStatementsCsvAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}

public sealed class FinanceTitleListItemDto
{
    public Guid Id { get; init; }
    public string Direction { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateOnly DueDate { get; init; }
    public decimal OriginalAmount { get; init; }
    public decimal OpenAmount { get; init; }
    public string Description { get; init; } = string.Empty;
}

public sealed class BalanceProjectionClientDto
{
    public decimal ExpectedReceivable { get; init; }
    public decimal ExpectedPayable { get; init; }
    public decimal RealizedReceivable { get; init; }
    public decimal RealizedPayable { get; init; }
}

public sealed class CashFlowDayClientDto
{
    public DateOnly Date { get; init; }
    public decimal RealizedInflow { get; init; }
    public decimal RealizedOutflow { get; init; }
    public decimal NetRealized { get; init; }
    public decimal ExpectedReceivable { get; init; }
    public decimal ExpectedPayable { get; init; }
}

public sealed class CashFlowReportClientDto
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyList<CashFlowDayClientDto> Days { get; init; } = Array.Empty<CashFlowDayClientDto>();
    public decimal TotalRealizedInflow { get; init; }
    public decimal TotalRealizedOutflow { get; init; }
}

public sealed class SimplifiedDreLineClientDto
{
    public string CategoryCode { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public decimal Expense { get; init; }
}

public sealed class SimplifiedDreReportClientDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public IReadOnlyList<SimplifiedDreLineClientDto> Lines { get; init; } = Array.Empty<SimplifiedDreLineClientDto>();
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetResult { get; init; }
}
