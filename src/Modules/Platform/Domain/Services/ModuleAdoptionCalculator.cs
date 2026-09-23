using Core.Domain.Entitlements;

namespace Platform.Domain.Services;

/// <summary>Input row for module adoption heatmap (10.3).</summary>
public sealed record TenantModuleAdoptionInput(
    Guid TenantId,
    string DisplayName,
    IReadOnlySet<CommercialModule> EffectiveModules);

/// <summary>Aggregated adoption snapshot for Super Admin.</summary>
public sealed record ModuleAdoptionSnapshot(
    IReadOnlyList<TenantAdoptionRow> Tenants,
    IReadOnlyList<ModuleAdoptionSummaryRow> ModuleSummaries);

/// <summary>One tenant row in the adoption matrix.</summary>
public sealed record TenantAdoptionRow(
    Guid TenantId,
    string DisplayName,
    IReadOnlyDictionary<CommercialModule, bool> Modules);

/// <summary>Adoption rate for one commercial module.</summary>
public sealed record ModuleAdoptionSummaryRow(
    CommercialModule Module,
    int EnabledCount,
    decimal AdoptionRate);

/// <summary>
/// Builds module adoption matrix from effective entitlements (plan ∪ add-ons ± flags).
/// </summary>
public static class ModuleAdoptionCalculator
{
    /// <summary>Computes tenant matrix and module summaries for non-cancelled tenants.</summary>
    public static ModuleAdoptionSnapshot Calculate(IEnumerable<TenantModuleAdoptionInput> tenants)
    {
        var tenantList = tenants.ToList();
        var modules = Enum.GetValues<CommercialModule>().OrderBy(m => (int)m).ToList();
        var total = tenantList.Count;

        var rows = new List<TenantAdoptionRow>(tenantList.Count);
        foreach (var tenant in tenantList.OrderBy(t => t.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            var cells = modules.ToDictionary(
                m => m,
                m => tenant.EffectiveModules.Contains(m));
            rows.Add(new TenantAdoptionRow(tenant.TenantId, tenant.DisplayName, cells));
        }

        var summaries = modules.Select(module =>
        {
            var enabled = tenantList.Count(t => t.EffectiveModules.Contains(module));
            var rate = total == 0
                ? 0m
                : decimal.Round((decimal)enabled / total, 4, MidpointRounding.AwayFromZero);
            return new ModuleAdoptionSummaryRow(module, enabled, rate);
        }).ToList();

        return new ModuleAdoptionSnapshot(rows, summaries);
    }
}
