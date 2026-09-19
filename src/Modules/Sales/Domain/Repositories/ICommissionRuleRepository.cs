using Sales.Domain.Entities;
using Sales.Domain.Enums;

namespace Sales.Domain.Repositories;

/// <summary>Persistence for tenant commission rules.</summary>
public interface ICommissionRuleRepository
{
    Task<IReadOnlyList<CommissionRule>> ListAllAsync(CancellationToken cancellationToken = default);

    Task<CommissionRule?> GetByRoleAndAppliesToAsync(CommissionRole role, CommissionAppliesTo appliesTo, CancellationToken cancellationToken = default);

    void Add(CommissionRule rule);

    void Update(CommissionRule rule);
}
