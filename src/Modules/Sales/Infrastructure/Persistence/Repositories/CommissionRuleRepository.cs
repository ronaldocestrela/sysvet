using Microsoft.EntityFrameworkCore;
using Sales.Domain.Entities;
using Sales.Domain.Enums;
using Sales.Domain.Repositories;

namespace Sales.Infrastructure.Persistence.Repositories;

/// <summary>EF-backed commission rule store.</summary>
public sealed class CommissionRuleRepository : ICommissionRuleRepository
{
    private readonly SalesDbContext _dbContext;

    public CommissionRuleRepository(SalesDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<CommissionRule>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.CommissionRules.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<CommissionRule?> GetByRoleAndAppliesToAsync(CommissionRole role, CommissionAppliesTo appliesTo, CancellationToken cancellationToken = default)
        => await _dbContext.CommissionRules.FirstOrDefaultAsync(r => r.Role == role && r.AppliesTo == appliesTo, cancellationToken);

    public void Add(CommissionRule rule) => _dbContext.CommissionRules.Add(rule);

    public void Update(CommissionRule rule) => _dbContext.CommissionRules.Update(rule);
}
