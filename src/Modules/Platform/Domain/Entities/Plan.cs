using Core.Domain;
using Core.Domain.Entitlements;

namespace Platform.Domain.Entities;

/// <summary>Global SaaS base plan catalog row (schema dbo).</summary>
public sealed class Plan : Entity
{
    /// <summary>Unique plan code (e.g. Starter).</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>Display name for Super Admin.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Monthly list price in BRL.</summary>
    public decimal MonthlyPrice { get; private set; }

    /// <summary>Modules bundled in the plan.</summary>
    public ICollection<PlanIncludedModule> IncludedModules { get; private set; } = new List<PlanIncludedModule>();

#pragma warning disable CS8618
    private Plan()
    {
    }
#pragma warning restore CS8618

    /// <summary>Creates a catalog plan with included modules.</summary>
    public static Result<Plan> Create(Guid id, string code, string name, decimal monthlyPrice, IEnumerable<CommercialModule> modules)
    {
        var normalizedCode = code?.Trim() ?? string.Empty;
        if (normalizedCode.Length < 2)
        {
            return Result.Failure<Plan>(ErrorCodes.Catalog.InvalidCode);
        }

        var displayName = name?.Trim() ?? string.Empty;
        if (displayName.Length < 2)
        {
            return Result.Failure<Plan>(ErrorCodes.Catalog.InvalidName);
        }

        if (monthlyPrice < 0)
        {
            return Result.Failure<Plan>(ErrorCodes.Catalog.InvalidPrice);
        }

        var plan = new Plan
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id,
            Code = normalizedCode,
            Name = displayName,
            MonthlyPrice = monthlyPrice,
            UpdatedAt = DateTimeOffset.UtcNow,
            RowVersion = new byte[8]
        };

        foreach (var module in modules.Distinct())
        {
            plan.IncludedModules.Add(new PlanIncludedModule(plan.Id, module));
        }

        return Result.Success(plan);
    }

    /// <summary>Returns module set for entitlement calculation.</summary>
    public IReadOnlyList<CommercialModule> GetModuleList() =>
        IncludedModules.Select(m => m.Module).ToList();
}
