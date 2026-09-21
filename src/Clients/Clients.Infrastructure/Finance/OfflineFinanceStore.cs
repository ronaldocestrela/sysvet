using Clients.Infrastructure.Sync;
using Core.Domain;
using Finance.Application.Reports;
using Finance.Application.Reports.Dtos;
using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Clients.Infrastructure.Finance;

/// <summary>
/// Reads finance titles from local SQLite; enqueues settle commands to outbox.
/// </summary>
public sealed class OfflineFinanceStore : IFinanceStore
{
    private readonly OfflineDbContext _dbContext;

    public OfflineFinanceStore(OfflineDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<FinanceTitleListItemDto>>> ListTitlesAsync(CancellationToken cancellationToken = default)
    {
        var titles = await _dbContext.FinancialTitles
            .AsNoTracking()
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<FinanceTitleListItemDto>>(titles.Select(t => new FinanceTitleListItemDto
        {
            Id = t.Id,
            Direction = t.Direction.ToString(),
            Status = t.Status.ToString(),
            DueDate = t.DueDate,
            OriginalAmount = t.OriginalAmount,
            OpenAmount = t.OpenAmount,
            Description = t.Description
        }).ToList());
    }

    public async Task<Result<BalanceProjectionClientDto>> GetProjectionAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var titles = await ListForStatementsAsync(from, to, cancellationToken);
        titles = titles.Where(t => t.Status != TitleStatus.Cancelled).ToList();

        decimal expectedReceivable = 0;
        decimal expectedPayable = 0;
        decimal realizedReceivable = 0;
        decimal realizedPayable = 0;

        foreach (var title in titles)
        {
            if (title.DueDate >= from && title.DueDate <= to)
            {
                if (title.Direction == TitleDirection.Receivable)
                {
                    expectedReceivable += title.OpenAmount;
                }
                else
                {
                    expectedPayable += title.OpenAmount;
                }
            }

            var net = NetRealizedInPeriod(title, from, to);
            if (title.Direction == TitleDirection.Receivable)
            {
                realizedReceivable += net;
            }
            else
            {
                realizedPayable += net;
            }
        }

        return Result.Success(new BalanceProjectionClientDto
        {
            ExpectedReceivable = expectedReceivable,
            ExpectedPayable = expectedPayable,
            RealizedReceivable = realizedReceivable,
            RealizedPayable = realizedPayable
        });
    }

    private static decimal NetRealizedInPeriod(FinancialTitle title, DateOnly from, DateOnly to)
    {
        decimal total = 0;
        foreach (var allocation in title.Allocations)
        {
            var paidDate = DateOnly.FromDateTime(allocation.PaidAt.UtcDateTime);
            if (paidDate < from || paidDate > to)
            {
                continue;
            }

            total += allocation.Kind == AllocationKind.Settlement ? allocation.Amount : -allocation.Amount;
        }

        return total;
    }

    public async Task<Result> SettleTitleAsync(Guid titleId, decimal amount, string method, CancellationToken cancellationToken = default)
    {
        var title = await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .FirstOrDefaultAsync(t => t.Id == titleId, cancellationToken);

        if (title is null)
        {
            return Result.Failure(new Error("Finance.TitleNotFound", "Título não encontrado localmente."));
        }

        var outboxId = Guid.NewGuid();
        var correlation = outboxId;
        var settle = title.Allocate(amount, DateTimeOffset.UtcNow, method, correlation);
        if (settle.IsFailure)
        {
            return settle;
        }

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            Type = "SettleFinancialTitleCommand",
            Payload = OutboxPayloadFactory.SettleFinancialTitle(titleId, amount, method, null, outboxId),
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<CashFlowReportClientDto>> GetCashFlowAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var titles = await ListForStatementsAsync(from, to, cancellationToken);
        var statement = FinancialStatementCalculator.BuildCashFlow(from, to, titles);
        if (statement.IsFailure)
        {
            return Result.Failure<CashFlowReportClientDto>(statement.Error);
        }

        var dto = FinanceReportMapper.ToDto(statement.Value);
        return Result.Success(MapCashFlow(dto));
    }

    public async Task<Result<SimplifiedDreReportClientDto>> GetSimplifiedDreAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);
        var titles = await ListForStatementsAsync(from, to, cancellationToken);
        var categories = await _dbContext.FinancialCategories.AsNoTracking().ToDictionaryAsync(c => c.Id, cancellationToken);
        var statement = FinancialStatementCalculator.BuildSimplifiedDre(year, month, titles, categories);
        if (statement.IsFailure)
        {
            return Result.Failure<SimplifiedDreReportClientDto>(statement.Error);
        }

        return Result.Success(MapDre(FinanceReportMapper.ToDto(statement.Value)));
    }

    public async Task<Result<byte[]>> ExportStatementsCsvAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var monthValidation = FinancialStatementCalculator.ValidateFullCalendarMonth(from, to);
        if (monthValidation.IsFailure)
        {
            return Result.Failure<byte[]>(monthValidation.Error);
        }

        var cashFlow = await GetCashFlowAsync(from, to, cancellationToken);
        if (cashFlow.IsFailure)
        {
            return Result.Failure<byte[]>(cashFlow.Error);
        }

        var dre = await GetSimplifiedDreAsync(from.Year, from.Month, cancellationToken);
        if (dre.IsFailure)
        {
            return Result.Failure<byte[]>(dre.Error);
        }

        var apiCashFlow = MapToApiCashFlow(cashFlow.Value);
        var apiDre = MapToApiDre(dre.Value);
        return Result.Success(FinanceStatementCsvExporter.Export(apiCashFlow, apiDre));
    }

    private async Task<IReadOnlyList<FinancialTitle>> ListForStatementsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var allocations = await _dbContext.TitleAllocations.AsNoTracking().ToListAsync(cancellationToken);
        var paidAllocationTitleIds = allocations
            .Where(a =>
            {
                var paidDate = DateOnly.FromDateTime(a.PaidAt.UtcDateTime);
                return paidDate >= from && paidDate <= to;
            })
            .Select(a => a.FinancialTitleId)
            .Distinct()
            .ToList();

        return await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .AsNoTracking()
            .Where(t =>
                (t.IssueDate >= from && t.IssueDate <= to)
                || (t.DueDate >= from && t.DueDate <= to)
                || paidAllocationTitleIds.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    private static CashFlowReportClientDto MapCashFlow(CashFlowReportDto dto) =>
        new()
        {
            From = dto.From,
            To = dto.To,
            Days = dto.Days.Select(d => new CashFlowDayClientDto
            {
                Date = d.Date,
                RealizedInflow = d.RealizedInflow,
                RealizedOutflow = d.RealizedOutflow,
                NetRealized = d.NetRealized,
                ExpectedReceivable = d.ExpectedReceivable,
                ExpectedPayable = d.ExpectedPayable
            }).ToList(),
            TotalRealizedInflow = dto.TotalRealizedInflow,
            TotalRealizedOutflow = dto.TotalRealizedOutflow
        };

    private static SimplifiedDreReportClientDto MapDre(SimplifiedDreReportDto dto) =>
        new()
        {
            Year = dto.Year,
            Month = dto.Month,
            Lines = dto.Lines.Select(l => new SimplifiedDreLineClientDto
            {
                CategoryCode = l.CategoryCode,
                CategoryName = l.CategoryName,
                Revenue = l.Revenue,
                Expense = l.Expense
            }).ToList(),
            TotalRevenue = dto.TotalRevenue,
            TotalExpense = dto.TotalExpense,
            NetResult = dto.NetResult
        };

    private static CashFlowReportDto MapToApiCashFlow(CashFlowReportClientDto dto) =>
        new()
        {
            From = dto.From,
            To = dto.To,
            Days = dto.Days.Select(d => new CashFlowDayDto
            {
                Date = d.Date,
                RealizedInflow = d.RealizedInflow,
                RealizedOutflow = d.RealizedOutflow,
                NetRealized = d.NetRealized,
                ExpectedReceivable = d.ExpectedReceivable,
                ExpectedPayable = d.ExpectedPayable
            }).ToList(),
            TotalRealizedInflow = dto.TotalRealizedInflow,
            TotalRealizedOutflow = dto.TotalRealizedOutflow
        };

    private static SimplifiedDreReportDto MapToApiDre(SimplifiedDreReportClientDto dto) =>
        new()
        {
            Year = dto.Year,
            Month = dto.Month,
            Lines = dto.Lines.Select(l => new SimplifiedDreLineDto
            {
                CategoryCode = l.CategoryCode,
                CategoryName = l.CategoryName,
                Revenue = l.Revenue,
                Expense = l.Expense
            }).ToList(),
            TotalRevenue = dto.TotalRevenue,
            TotalExpense = dto.TotalExpense,
            NetResult = dto.NetResult
        };
}
