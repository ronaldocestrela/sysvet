using Finance.Domain.Entities;

namespace Finance.Domain.Repositories;

/// <summary>
/// Persistence for cost centers.
/// </summary>
public interface ICostCenterRepository
{
    Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CostCenter?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<CostCenter>> ListAsync(CancellationToken cancellationToken);

    void Add(CostCenter costCenter);

    void Update(CostCenter costCenter);
}
