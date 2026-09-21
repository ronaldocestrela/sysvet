using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Models;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

public sealed class FinancialTitleRepository : IFinancialTitleRepository
{
    private readonly FinanceDbContext _dbContext;

    public FinancialTitleRepository(FinanceDbContext dbContext) => _dbContext = dbContext;

    public void Add(FinancialTitle title) => _dbContext.FinancialTitles.Add(title);

    public void Update(FinancialTitle title)
    {
        var autoDetect = _dbContext.ChangeTracker.AutoDetectChangesEnabled;
        _dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        try
        {
            var trackedTitle = _dbContext.ChangeTracker.Entries<FinancialTitle>()
                .FirstOrDefault(e => e.Entity.Id == title.Id);

            if (trackedTitle is null)
            {
                _dbContext.FinancialTitles.Update(title);
                return;
            }

            var knownAllocationIds = _dbContext.ChangeTracker.Entries<TitleAllocation>()
                .Select(e => e.Entity.Id)
                .ToHashSet();

            foreach (var allocation in title.Allocations)
            {
                if (knownAllocationIds.Contains(allocation.Id))
                {
                    continue;
                }

                _dbContext.TitleAllocations.Add(allocation);
            }
        }
        finally
        {
            _dbContext.ChangeTracker.AutoDetectChangesEnabled = autoDetect;
        }
    }

    public async Task<FinancialTitle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<FinancialTitle?> GetBySourceAsync(
        TitleSourceType sourceType,
        Guid sourceId,
        string installmentKey,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .FirstOrDefaultAsync(
                t => t.SourceType == sourceType && t.SourceId == sourceId && t.SourceInstallmentKey == installmentKey,
                cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialTitle>> ListBySourceIdAsync(
        TitleSourceType sourceType,
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FinancialTitles
            .Include(t => t.Allocations)
            .Where(t => t.SourceType == sourceType && t.SourceId == sourceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialTitle>> ListAsync(
        TitleDirection? direction,
        TitleStatus? status,
        PartyKind? partyKind,
        Guid? partyId,
        DateOnly? dueFrom,
        DateOnly? dueTo,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.FinancialTitles.Include(t => t.Allocations).AsQueryable();

        if (direction.HasValue)
        {
            query = query.Where(t => t.Direction == direction.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (partyKind.HasValue)
        {
            query = query.Where(t => t.PartyKind == partyKind.Value);
        }

        if (partyId.HasValue)
        {
            query = query.Where(t => t.PartyId == partyId.Value);
        }

        if (dueFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueFrom.Value);
        }

        if (dueTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueTo.Value);
        }

        return await query.OrderBy(t => t.DueDate).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FinancialTitle>> ListForStatementsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var allocations = await _dbContext.TitleAllocations.AsNoTracking().ToListAsync(cancellationToken);
        var titleIdsWithPaidAllocations = allocations
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
            .Where(t =>
                (t.IssueDate >= from && t.IssueDate <= to)
                || (t.DueDate >= from && t.DueDate <= to)
                || titleIdsWithPaidAllocations.Contains(t.Id))
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CardSettlementSnapshot>> ListCardSettlementSnapshotsAsync(CancellationToken cancellationToken)
    {
        var cardMethods = new[] { "DebitCard", "CreditCard" };

        return await _dbContext.TitleAllocations
            .AsNoTracking()
            .Where(a => a.Kind == AllocationKind.Settlement
                        && a.ExternalReference != null
                        && cardMethods.Contains(a.Method))
            .Select(a => new CardSettlementSnapshot(a.Id, a.ExternalReference!, a.Amount, a.Method))
            .ToListAsync(cancellationToken);
    }
}
