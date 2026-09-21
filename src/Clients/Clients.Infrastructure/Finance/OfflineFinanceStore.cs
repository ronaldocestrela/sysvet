using Clients.Infrastructure.Sync;
using Core.Domain;
using Finance.Domain.Enums;
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
        var titles = await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .AsNoTracking()
            .Where(t => t.DueDate >= from && t.DueDate <= to && t.Status != TitleStatus.Cancelled)
            .ToListAsync(cancellationToken);

        decimal expectedReceivable = 0;
        decimal expectedPayable = 0;
        decimal realizedReceivable = 0;
        decimal realizedPayable = 0;

        foreach (var title in titles)
        {
            if (title.Direction == TitleDirection.Receivable)
            {
                expectedReceivable += title.OpenAmount;
                realizedReceivable += title.Allocations
                    .Where(a => a.Kind == AllocationKind.Settlement)
                    .Sum(a => a.Amount);
            }
            else
            {
                expectedPayable += title.OpenAmount;
                realizedPayable += title.Allocations
                    .Where(a => a.Kind == AllocationKind.Settlement)
                    .Sum(a => a.Amount);
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
}
