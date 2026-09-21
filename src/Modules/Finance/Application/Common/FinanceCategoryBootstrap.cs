using Finance.Domain.Entities;
using Finance.Domain.Enums;
using Finance.Domain.Repositories;

namespace Finance.Application.Common;

/// <summary>
/// Ensures system categories exist before creating titles.
/// </summary>
public static class FinanceCategoryBootstrap
{
    public static async Task<FinancialCategory> EnsureSystemCategoryAsync(
        IFinancialCategoryRepository repository,
        string code,
        string name,
        CategoryDirection direction,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCodeAsync(code, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var category = FinancialCategory.CreateSystem(code, name, direction);
        repository.Add(category);
        return category;
    }
}
