using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

public sealed class FinancialCategoryRepository : IFinancialCategoryRepository
{
    private readonly FinanceDbContext _dbContext;

    public FinancialCategoryRepository(FinanceDbContext dbContext) => _dbContext = dbContext;

    public void Add(FinancialCategory category) => _dbContext.FinancialCategories.Add(category);

    public void Update(FinancialCategory category) => _dbContext.FinancialCategories.Update(category);

    public async Task<FinancialCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _dbContext.FinancialCategories.FindAsync([id], cancellationToken);

    public async Task<FinancialCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await _dbContext.FinancialCategories.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlyList<FinancialCategory>> ListAsync(CancellationToken cancellationToken)
        => await _dbContext.FinancialCategories.OrderBy(c => c.Code).ToListAsync(cancellationToken);
}
