using Finance.Domain.Entities;

namespace Finance.Domain.Repositories;

/// <summary>
/// Persistence for financial categories.
/// </summary>
public interface IFinancialCategoryRepository
{
    Task<FinancialCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<FinancialCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    Task<IReadOnlyList<FinancialCategory>> ListAsync(CancellationToken cancellationToken);

    void Add(FinancialCategory category);

    void Update(FinancialCategory category);
}
