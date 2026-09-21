using Core.Application.Sync;
using Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Sync;

/// <summary>
/// Supplies finance entities for sync pull.
/// </summary>
public sealed class FinanceSyncChangeFeedContributor : ISyncChangeFeedContributor
{
    private readonly FinanceDbContext _dbContext;

    public FinanceSyncChangeFeedContributor(FinanceDbContext dbContext) => _dbContext = dbContext;

    public async Task<SyncContributorChanges> ReadChangesAsync(DateTimeOffset since, int take, CancellationToken cancellationToken)
    {
        // SQLite does not translate DateTimeOffset comparisons reliably; filter in memory after load.
        var titles = await _dbContext.FinancialTitles
            .AsNoTracking()
            .Include(t => t.Allocations)
            .ToListAsync(cancellationToken);
        var titlePage = PageByUpdatedAt(titles, since, take);

        var categories = await _dbContext.FinancialCategories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var categoryPage = PageByUpdatedAt(categories, since, take);

        var costCenters = await _dbContext.CostCenters
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var costCenterPage = PageByUpdatedAt(costCenters, since, take);

        var maxUpdated = since;
        maxUpdated = Max(maxUpdated, titlePage.MaxUpdated);
        maxUpdated = Max(maxUpdated, categoryPage.MaxUpdated);
        maxUpdated = Max(maxUpdated, costCenterPage.MaxUpdated);

        return new SyncContributorChanges
        {
            FinanceTitles = titlePage.Items.Select(t => new SyncFinanceTitleDto
            {
                Id = t.Id,
                Direction = t.Direction.ToString(),
                Status = t.Status.ToString(),
                SourceType = t.SourceType.ToString(),
                SourceId = t.SourceId,
                SourceInstallmentKey = t.SourceInstallmentKey,
                PartyKind = t.PartyKind.ToString(),
                PartyId = t.PartyId,
                CategoryId = t.CategoryId,
                CostCenterId = t.CostCenterId,
                IssueDate = t.IssueDate,
                DueDate = t.DueDate,
                OriginalAmount = t.OriginalAmount,
                Description = t.Description,
                UpdatedAt = t.UpdatedAt,
                Allocations = t.Allocations.Select(a => new SyncTitleAllocationDto
                {
                    Id = a.Id,
                    FinancialTitleId = a.FinancialTitleId,
                    Amount = a.Amount,
                    PaidAt = a.PaidAt,
                    Method = a.Method,
                    ExternalReference = a.ExternalReference,
                    CorrelationId = a.CorrelationId,
                    Kind = a.Kind.ToString(),
                    UpdatedAt = a.UpdatedAt
                }).ToList()
            }).ToList(),
            FinanceCategories = categoryPage.Items.Select(c => new SyncFinanceCategoryDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Direction = c.Direction.ToString(),
                IsSystem = c.IsSystem,
                IsActive = c.IsActive,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            FinanceCostCenters = costCenterPage.Items.Select(c => new SyncFinanceCostCenterDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                IsActive = c.IsActive,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            MaxUpdatedAt = maxUpdated,
            HasMore = titlePage.HasMore || categoryPage.HasMore || costCenterPage.HasMore
        };
    }

    private static DateTimeOffset Max(DateTimeOffset a, DateTimeOffset b) => a > b ? a : b;

    private static (IReadOnlyList<T> Items, DateTimeOffset MaxUpdated, bool HasMore) PageByUpdatedAt<T>(
        IEnumerable<T> source,
        DateTimeOffset since,
        int take)
        where T : Core.Domain.Entity
    {
        var candidates = source.Where(x => x.UpdatedAt > since).OrderBy(x => x.UpdatedAt).Take(take + 1).ToList();
        var hasMore = candidates.Count > take;
        if (hasMore)
        {
            candidates = candidates.Take(take).ToList();
        }

        var max = since;
        foreach (var item in candidates)
        {
            if (item.UpdatedAt > max)
            {
                max = item.UpdatedAt;
            }
        }

        return (candidates, max, hasMore);
    }
}
