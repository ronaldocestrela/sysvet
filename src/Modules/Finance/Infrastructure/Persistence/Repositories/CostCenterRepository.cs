using Finance.Domain.Entities;
using Finance.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence.Repositories;

public sealed class CostCenterRepository : ICostCenterRepository
{
    private readonly FinanceDbContext _dbContext;

    public CostCenterRepository(FinanceDbContext dbContext) => _dbContext = dbContext;

    public void Add(CostCenter costCenter) => _dbContext.CostCenters.Add(costCenter);

    public void Update(CostCenter costCenter) => _dbContext.CostCenters.Update(costCenter);

    public async Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _dbContext.CostCenters.FindAsync([id], cancellationToken);

    public async Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken)
        => await _dbContext.CostCenters.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

    public async Task<IReadOnlyList<CostCenter>> ListAsync(CancellationToken cancellationToken)
        => await _dbContext.CostCenters.OrderBy(c => c.Code).ToListAsync(cancellationToken);
}
