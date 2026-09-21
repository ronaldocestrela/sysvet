using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

public sealed class CardReconciliationRepository : ICardReconciliationRepository
{
    private readonly FinanceDbContext _dbContext;

    public CardReconciliationRepository(FinanceDbContext dbContext) => _dbContext = dbContext;

    public void Add(CardReconciliationBatch batch) => _dbContext.CardReconciliationBatches.Add(batch);

    public async Task<CardReconciliationBatch?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.CardReconciliationBatches
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CardReconciliationBatch>> ListAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CardReconciliationBatches
            .Include(b => b.Lines)
            .AsNoTracking()
            .OrderByDescending(b => b.ImportedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> ListMatchedAllocationIdsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CardReconciliationLines
            .AsNoTracking()
            .Where(l => l.MatchedAllocationId != null)
            .Select(l => l.MatchedAllocationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
